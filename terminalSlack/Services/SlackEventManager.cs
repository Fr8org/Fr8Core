using System;
using System.Collections.Generic;
using System.Linq;
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

        private bool _disposed;

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
            lock (_locker)
            {
                if (_disposed)
                {
                    return Task.FromResult(0);
                }
                Unsubscribe(activityId);
                SlackClientWrapper client;
                var teamId = token.ExternalDomainId;
                if (!_clientsByTeamId.TryGetValue(teamId, out client))
                {
                    //This team doesn't have subscription yet - create a new subscription
                    client = new SlackClientWrapper(token.Token, token.ExternalDomainId);
                    client.MessageReceived += OnMessageReceived;
                    client.Connect();
                    _clientsByTeamId.Add(teamId, client);
                    client.ClientConnectOperation.Task.ContinueWith(x => OnSubscriptionFailed(client), TaskContinuationOptions.NotOnRanToCompletion);
                }
                client.Subscribe(activityId);
                _teamIdByActivity[activityId] = teamId;
                return client.ClientConnectOperation.Task;
            }
        }

        private void OnSubscriptionFailed(SlackClientWrapper client)
        {
            lock (_locker)
            {
                foreach (var acitivityId in client.SubscribedActivities)
                {
                    _teamIdByActivity.Remove(acitivityId);
                }
                _clientsByTeamId.Remove(client.TeamId);
            }
        }

        public void Unsubscribe(Guid activityId)
        {
            lock (_locker)
            {
                if (_disposed)
                {
                    return;
                }
                string existingTeamId;
                if (_teamIdByActivity.TryGetValue(activityId, out existingTeamId))
                {
                    _teamIdByActivity.Remove(activityId);
                }
                SlackClientWrapper client;
                if (!string.IsNullOrEmpty(existingTeamId) && _clientsByTeamId.TryGetValue(existingTeamId, out client))
                {
                    //We've removed last subscription - disconnect from RTM websocket and remove it
                    if (client.Unsubsribe(activityId))
                    {
                        _clientsByTeamId.Remove(existingTeamId);
                        client.MessageReceived -= OnMessageReceived;
                        client.Dispose();
                    }
                }
            }
        }

        public void Dispose()
        {
            lock (_locker)
            {
                if (_disposed)
                {
                    return;
                }
                _disposed = true;
                //This is to perform graceful exit in case of terminal shutdown
                foreach (var client in _clientsByTeamId.Values)
                {
                    client.Dispose();
                }
            }
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
    }
}