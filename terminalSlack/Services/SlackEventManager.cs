using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Data.Entities;
using Hub.Managers.APIManagers.Transmitters.Restful;
using ServiceStack.Text;
using SlackAPI;
using terminalSlack.Interfaces;
using Utilities.Configuration.Azure;
using Utilities.Logging;

namespace terminalSlack.Services
{
    public class SlackEventManager : ISlackEventManager
    {
        private readonly IRestfulServiceClient _resfultClient;

        private readonly Dictionary<string, SlackClientWrapper> _clientsByTeamId = new Dictionary<string, SlackClientWrapper>();

        private readonly Dictionary<Guid, string> _teamIdByActivity = new Dictionary<Guid, string>();
        
        private readonly object _locker = new object();

        private readonly Uri _eventsUri;

        private int _disposed;

        public SlackEventManager(IRestfulServiceClient resfultClient)
        {
            if (resfultClient == null)
            {
                throw new ArgumentNullException(nameof(resfultClient));
            }
            _resfultClient = resfultClient;
            _eventsUri = new Uri($"{CloudConfigurationManager.GetSetting("terminalSlack.TerminalEndpoint")}/terminals/terminalslack/events", UriKind.Absolute);
        }

        public Task Subscribe(AuthorizationTokenDO token, Guid activityId)
        {
            if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
            {
                //It means that we disposed our registry already probably due to terminal shutdown
                return Task.FromResult(0);
            }
            if (string.IsNullOrEmpty(token.ExternalDomainId))
            {
                throw new ArgumentException("Authorization token doesn't contain info about Slack user's team. Try to revoke existing Slack token and reauthorize again", nameof(token));
            }
            if (activityId == Guid.Empty)
            {
                throw new ArgumentException("Can't create subscription for empty activity Id", nameof(activityId));
            }
            lock (_locker)
            {
                Unsubscribe(activityId);
                //This team already have subscription - just return it
                SlackClientWrapper client;
                var teamId = token.ExternalDomainId;
                if (_clientsByTeamId.TryGetValue(teamId, out client))
                {
                    client.SubscribedActivities.Add(activityId);
                    _teamIdByActivity[activityId] = teamId;
                    return client.ClientConnectOperation.Task;
                }
                //This team doesn't have subscription yet - create a new subscription
                client = new SlackClientWrapper
                {
                    ClientConnectOperation = new TaskCompletionSource<int>(),
                    SubscribedActivities = new HashSet<Guid> { activityId },
                    Client = new SlackSocketClient(token.Token)
                };
                _clientsByTeamId.Add(teamId, client);
                _teamIdByActivity[activityId] = teamId;
                client.Client.Connect(x => OnLoginResponse(client, x, teamId));
                return client.ClientConnectOperation.Task;
            }
        }

        private void OnLoginResponse(SlackClientWrapper client, LoginResponse loginResponse, string teamId)
        {
            if (!loginResponse.ok)
            {
                client.ClientConnectOperation.SetException(new ApplicationException($"Failed to start Slack RTM session. Error code - {loginResponse.error}"));
                ClearClientSubscriptions(teamId);
                return;
            }
            client.Client.MessageReceived += OnIgnoredMessageReceived;
            client.ClientConnectOperation.SetResult(0);
        }

        private void ClearClientSubscriptions(string teamId)
        {
            lock (_locker)
            {
                SlackClientWrapper client;
                if (string.IsNullOrEmpty(teamId) || !_clientsByTeamId.TryGetValue(teamId, out client))
                {
                    return;
                }
                foreach (var activityId in client.SubscribedActivities)
                {
                    _teamIdByActivity.Remove(activityId);
                }
                if (!client.ClientConnectOperation.Task.IsCompleted)
                {
                    client.ClientConnectOperation.SetCanceled();
                }
                _clientsByTeamId.Remove(teamId);
            }
        }

        public void Unsubscribe(Guid activityId)
        {
            lock (_locker)
            {
                string existingTeamId;
                if (_teamIdByActivity.TryGetValue(activityId, out existingTeamId))
                {
                    _teamIdByActivity.Remove(activityId);
                }
                SlackClientWrapper client;
                if (!string.IsNullOrEmpty(existingTeamId) && _clientsByTeamId.TryGetValue(existingTeamId, out client))
                {
                    client.SubscribedActivities.Remove(activityId);
                    //We've removed last subscription - disconnect from RTM websocket and remove it
                    if (client.SubscribedActivities.Count == 0)
                    {
                        _clientsByTeamId.Remove(existingTeamId);
                        if (!client.ClientConnectOperation.Task.IsCompleted)
                        {
                            client.ClientConnectOperation.SetCanceled();
                        }
                        client.Client.MessageReceived -= OnIgnoredMessageReceived;
                        client.Client.MessageReceived -= OnMessageReceived;
                        try
                        {
                            client.Client.CloseSocket();
                        }
                        catch (Exception ex)
                        {
                            Logger.GetLogger().Error("Failed to unsubscribe from Slack client events. Probably subscriptions was not completed prior to unsubscribe request", ex);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                //Already disposed
                return;
            }
            lock (_locker)
            {
                //This is to perform graceful exit in case of terminal shutdown
                foreach (var client in _clientsByTeamId.Values)
                {
                    client.Client.MessageReceived -= OnIgnoredMessageReceived;
                    client.Client.MessageReceived -= OnMessageReceived;
                    client.Client.CloseSocket();
                    client.ClientConnectOperation.SetCanceled();
                }
            }
        }

        private void OnIgnoredMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //After we successfully login we recieve last message sent or recieved by current user so we should ignore it
            var client = (SlackSocketClient)sender;
            client.MessageReceived -= OnIgnoredMessageReceived;
            client.MessageReceived += OnMessageReceived;
        }

        private async void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //The naming conventions of message property is for backwards compatibility with existing event processing logic
            var valuePairs = new List<KeyValuePair<string, string>>(8)
                             {
                                 new KeyValuePair<string, string>("team_id", e.WrappedMessage.TeamId),
                                 new KeyValuePair<string, string>("team_domain", e.WrappedMessage.TeamName),
                                 new KeyValuePair<string, string>("channel_id", e.WrappedMessage.ChannelId),
                                 new KeyValuePair<string, string>("channel_name", e.WrappedMessage.ChannelName),
                                 new KeyValuePair<string, string>("timestamp", e.WrappedMessage.Timestamp.ToUniversalTime().ToUnixTime().ToString()),
                                 new KeyValuePair<string, string>("user_id", e.WrappedMessage.UserId),
                                 new KeyValuePair<string, string>("user_name", e.WrappedMessage.UserName),
                                 new KeyValuePair<string, string>("text", e.WrappedMessage.Text)
                             };
            var encodedMessage = string.Join("&", valuePairs.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => $"{x.Key}={HttpUtility.UrlEncode(x.Value)}"));
            try
            {
                await _resfultClient.PostAsync(_eventsUri, content: encodedMessage);
            }
            catch (Exception ex)
            {
                Logger.GetLogger().Info($"Failed to post event from SlackEventMenager with following payload: {encodedMessage}", ex);
            }
        }

        private class SlackClientWrapper
        {
            public SlackSocketClient Client { get; set; }

            public TaskCompletionSource<int> ClientConnectOperation { get; set; }

            public HashSet<Guid> SubscribedActivities { get; set; }
        }
    }
}