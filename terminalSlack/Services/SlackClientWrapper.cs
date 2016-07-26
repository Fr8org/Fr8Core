using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fr8.Infrastructure.Utilities.Logging;
using ServiceStack;
using terminalSlack.RtmClient;
using terminalSlack.RtmClient.Entities;
using terminalSlack.RtmClient.Events;

namespace terminalSlack.Services
{
    internal sealed class SlackClientWrapper : IDisposable
    {
        public SlackRtmClient Client { get; set; }

        public LoginResponse SlackData { get; set; }

        public string UserName { get; set; }

        private readonly HashSet<Guid> _subscribedActivities;

        public event EventHandler<DataEventArgs<WrappedMessage>> MessageReceived;

        public SlackClientWrapper(string oAuthToken, string userName)
        {
            Client = new SlackRtmClient(oAuthToken);
            UserName = userName;
            Client.MessageReceived += ClientOnMessageReceived;
            _subscribedActivities = new HashSet<Guid>();
        }

        public async Task Connect()
        {
            var result = await Client.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
            if (SlackData == null)
            {
                //This delay is to skip processing of latest message that is received immediately after connect (this is likely a feature of RTM when it send you the latest message
                //sent or received by token owner)
                //I asked a question on SO http://stackoverflow.com/questions/36972563/how-to-stop-slack-from-sending-last-message-after-starting-rtm-session
                //track this to see if this message can be ignored completely
                await Task.Delay(TimeSpan.FromSeconds(1.0)).ConfigureAwait(false);
                SlackData = result;
            }
            if (!result.Ok)
            {
                throw new ApplicationException($"Failed to connect to Slack. Error code - '{result.Error.Code}', error message - '{result.Error.Message}'");
            }
        }
        /// <summary>
        /// Adds specified plan Id to subscription list
        /// </summary>
        public void Subscribe(Guid planId)
        {
            lock (_subscribedActivities)
            {
                _subscribedActivities.Add(planId);
            }
        }
        /// <summary>
        /// Removes specified plan Id from subscription list and returns value indicating whether any active subscription stil exists
        /// </summary>
        public bool Unsubsribe(Guid planId)
        {
            lock (_subscribedActivities)
            {
                _subscribedActivities.Remove(planId);
                return _subscribedActivities.Count == 0;
            }
        }

        public IEnumerable<Guid> SubscribedPlans
        {
            get
            {
                lock (_subscribedActivities)
                {
                    return _subscribedActivities.ToArray();
                }
            }
        }

        private void ClientOnMessageReceived(object sender, DataEventArgs<Message> e)
        {
            if (SlackData == null)
            {
                return;
            }
            if (e.Data.IsHidden)
            {
                Logger.GetLogger().Info($"SlackClientWrapper: hidden message is received for user {UserName} and won't go further");
                return;
            }
            MessageReceived?.Invoke(this, new DataEventArgs<WrappedMessage>(new WrappedMessage
            {
                TeamId = SlackData.Team.Id,
                TeamName = SlackData.Team.Name,
                Timestamp = e.Data.Timestamp,
                ChannelId = e.Data.ChannelId,
                ChannelName = SlackData.Channels.FirstOrDefault(x => x.Id == e.Data.ChannelId)?.Name,
                UserName = SlackData.Users.FirstOrDefault(x => x.Id == e.Data.UserId)?.Name,
                Text = e.Data.Text,
                UserId = e.Data.UserId
            }));
        }

        public void Dispose()
        {
            Client.MessageReceived -= ClientOnMessageReceived;
            Client.Dispose();
        }
    }

    public class WrappedMessage
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string ChannelId { get; set; }

        public string ChannelName { get; set; }

        public string TeamId { get; set; }

        public string TeamName { get; set; }

        public string Timestamp { get; set; }

        public string Text { get; set; }

    }
}