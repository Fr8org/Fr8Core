using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack;
using SlackAPI;
using Utilities.Logging;

namespace terminalSlack.Services
{
    internal sealed class SlackClientWrapper : IDisposable
    {
        public SlackSocketClient Client { get; set; }

        public string TeamId { get; private set; }

        public TaskCompletionSource<int> ClientConnectOperation { get; set; }

        private readonly HashSet<Guid> _subscribedActivities;

        private int _isConnecting;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        private int _firstMessageIsSkipped;

        public SlackClientWrapper(string oAuthToken, string teamId)
        {
            Client = new SlackSocketClient(oAuthToken);
            ClientConnectOperation = new TaskCompletionSource<int>();
            _subscribedActivities = new HashSet<Guid>();
            TeamId = teamId;
        }

        public Task Connect()
        {
            //If we already called Connect() then we should just return the result of the operation
            if (Interlocked.CompareExchange(ref _isConnecting, 1, 0) == 0)
            {
                Client.Connect(OnLoginResponse);
                Client.MessageReceived += ClientOnMessageReceived;
            }
            return ClientConnectOperation.Task;
        }

        private void ClientOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //After we successfully login we recieve last message sent or recieved by current user so we should ignore it
            if (Interlocked.CompareExchange(ref _firstMessageIsSkipped, 1, 0) != 0)
            {
                OnMessageReceived(e);
            }
        }
        /// <summary>
        /// Adds specified activity Id to subscription list
        /// </summary>
        public void Subscribe(Guid activityId)
        {
            lock (_subscribedActivities)
            {
                _subscribedActivities.Add(activityId);
            }
        }
        /// <summary>
        /// Removes specified activity Id from subscription list and returns value indicating whether any active subscription stil exists
        /// </summary>
        public bool Unsubsribe(Guid activityId)
        {
            lock (_subscribedActivities)
            {
                _subscribedActivities.Remove(activityId);
                return _subscribedActivities.Count == 0;
            }
        }

        public IEnumerable<Guid> SubscribedActivities
        {
            get
            {
                lock (_subscribedActivities)
                {
                    return _subscribedActivities.ToArray();
                }
            }
        }

        private void OnLoginResponse(LoginResponse loginResponse)
        {
            if (loginResponse.ok)
            {
                ClientConnectOperation.SetResult(0);
            }
            else
            {
                ClientConnectOperation.SetException(new ApplicationException($"Failed to start Slack RTM session. Error code - {loginResponse.error}"));
            }
        }

        private void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceived(this, e);
        }

        public void Dispose()
        {
            var cancelTask = false;
            if (Interlocked.CompareExchange(ref _isConnecting, 1, 0) == 1)
            {
                //This means Connect() method was called - close socket
                try
                {
                    Client.CloseSocket();
                    cancelTask = true;
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().Info("Failed to gracefully shutdown Slack connection", ex);
                }
            }
            if (cancelTask && !ClientConnectOperation.Task.IsCompleted)
            {
                ClientConnectOperation.SetCanceled();
            }
        }
    }
}