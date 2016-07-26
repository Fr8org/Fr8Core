using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.TerminalBase.Models;
using terminalSlack.Interfaces;
using terminalSlack.RtmClient;

namespace terminalSlack.Services
{
    public class SlackEventManager : ISlackEventManager
    {
        private readonly IRestfulServiceClient _resfultClient;

        private readonly Dictionary<string, SlackClientWrapper> _clientsByUserName = new Dictionary<string, SlackClientWrapper>();

        private readonly Dictionary<Guid, string> _accountsByPlanId = new Dictionary<Guid, string>();
        
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

        public Task Subscribe(AuthorizationToken token, Guid planId)
        {
            lock (_locker)
            {
                if (_disposed)
                {
                    Logger.GetLogger().Warn($"SlackEventManager: can't subscribe to disposed object. User = {token.ExternalAccountId}, PlanId = {planId}");
                    return Task.FromResult(0);
                }
                Unsubscribe(planId);
                SlackClientWrapper client;
                var userName = token.ExternalAccountId;
                if (!_clientsByUserName.TryGetValue(userName, out client))
                {
                    Logger.GetLogger().Info($"SlackEventManager: creating new subscription and opening socket for user {token.ExternalAccountId}");
                    //This user doesn't have subscription yet - create a new subscription
                    client = new SlackClientWrapper(token.Token, userName);
                    client.MessageReceived += OnMessageReceived;
                    _clientsByUserName.Add(userName, client);
                }
                else
                {
                    Logger.GetLogger().Info($"SlackEventManager: client for user {token.ExternalAccountId} already exists");
                }
                Logger.GetLogger().Info($"SlackEventManager: subscribing Plan {planId} to events from user {token.ExternalAccountId}");
                client.Subscribe(planId);
                _accountsByPlanId[planId] = userName;
                var result = client.Connect();
                result.ContinueWith(x => { if (x.IsFaulted) OnSubscriptionFailed(client, token.ExternalAccountId, x.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
                return result;
            }
        }

        private void OnSubscriptionFailed(SlackClientWrapper client, string externalAccountId, AggregateException exception)
        {
            Logger.GetLogger().Warn($"SlackEventManager: subscription fail for user {externalAccountId}. Exception - {exception}");
            lock (_locker)
            {
                foreach (var planId in client.SubscribedPlans)
                {
                    _accountsByPlanId.Remove(planId);
                }
                _clientsByUserName.Remove(client.SlackData.Self.Name);
                client.Dispose();
            }
        }

        public void Unsubscribe(Guid planId)
        {
            //Logger.GetLogger().Info($"SlackEventManager: usubscribing in thread {Thread.CurrentThread.ManagedThreadId}");
            Logger.GetLogger().Info($"SlackEventManager: usubscribing plan {planId}");
            lock (_locker)
            {
                if (_disposed)
                {
                    return;
                }
                string existingUserName;
                if (_accountsByPlanId.TryGetValue(planId, out existingUserName))
                {
                    _accountsByPlanId.Remove(planId);
                }
                SlackClientWrapper client;
                if (!string.IsNullOrEmpty(existingUserName) && _clientsByUserName.TryGetValue(existingUserName, out client))
                {
                    //We've removed last subscription - disconnect from RTM websocket and remove it
                    if (client.Unsubsribe(planId))
                    {
                        //Logger.GetLogger().Info("SlackEventManager: unsubscribing closes socket");
                        Logger.GetLogger().Info($"SlackEventManager: unsubscribing of plan {planId} closes client for user {existingUserName}");
                        _clientsByUserName.Remove(existingUserName);
                        client.MessageReceived -= OnMessageReceived;
                        client.Dispose();
                    }
                }
            }
        }

        public void Dispose()
        {
            //Logger.GetLogger().Info("SlackEventManager: Dispose() closes all clients");
            Logger.GetLogger().Info("SlackEventManager: Dispose() closes all clients");
            lock (_locker)
            {
                if (_disposed)
                {
                    return;
                }
                _disposed = true;
                //This is to perform graceful exit in case of terminal shutdown
                foreach (var client in _clientsByUserName.Values)
                {
                    client.Dispose();
                }
            }
        }

        private async void OnMessageReceived(object sender, DataEventArgs<WrappedMessage> e)
        {
            Logger.GetLogger().Info($"SlackEventManager: message '{e.Data.Text}' is received from {e.Data.UserName}");
            //The naming conventions of message property is for backwards compatibility with existing event processing logic
            var client = (SlackClientWrapper)sender;
            var valuePairs = new List<KeyValuePair<string, string>>
                             {
                                 new KeyValuePair<string, string>("team_id", e.Data.TeamId),
                                 new KeyValuePair<string, string>("team_domain", e.Data.TeamName),
                                 new KeyValuePair<string, string>("channel_id", e.Data.ChannelId),
                                 new KeyValuePair<string, string>("channel_name", e.Data.ChannelName),
                                 new KeyValuePair<string, string>("timestamp", e.Data.Timestamp),
                                 new KeyValuePair<string, string>("user_id", e.Data.UserId),
                                 new KeyValuePair<string, string>("user_name", e.Data.UserName),
                                 new KeyValuePair<string, string>("text", e.Data.Text),
                                 new KeyValuePair<string, string>("owner_name", client.UserName)
                             };
            var encodedMessage = string.Join("&", valuePairs.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => $"{x.Key}={HttpUtility.UrlEncode(x.Value)}"));
            try
            {
                await _resfultClient.PostAsync(_eventsUri, content: encodedMessage).ConfigureAwait(false);
                Logger.GetLogger().Info($"SlackEventManager: message was posted to Slack terminal for user's {client.UserName} active plans");
            }
            catch (Exception ex)
            {
                //Logger.GetLogger().Info($"Failed to post event from SlackEventMenager with following payload: {encodedMessage}", ex);
                Logger.GetLogger().Error($"SlackEventManager: failed to post event to terminal Slack llowing payload: {encodedMessage}. {ex}");
            }
        }
    }
}