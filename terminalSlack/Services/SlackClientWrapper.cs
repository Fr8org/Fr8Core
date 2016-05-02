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
                //Logger.GetLogger().Info("SlackClientWrapper: connecting to socket...");
                Logger.LogInfo($"SlackClientWrapper: connecting to socket...");
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
                //Logger.GetLogger().Info("SlackClientWrapper: message received");
                Logger.LogInfo("SlackClientWrapper: message received");
                OnMessageReceived(e);
            }
        }
        /// <summary>
        /// Adds specified activity Id to subscription list
        /// </summary>
        public void Subscribe(Guid activityId)
        {
            //Logger.GetLogger().Info($"SlackClientWrapper: mark activity {activityId} as subscribed");
            Logger.LogInfo("SlackClientWrapper: message received");
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
            //Logger.GetLogger().Info($"SlackClientWrapper: mark activity {activityId} as unsubscribed");
            Logger.LogInfo($"SlackClientWrapper: mark activity {activityId} as unsubscribed");
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
                //Logger.GetLogger().Info("SlackClientWrapper: received succesfull login response");
                Logger.LogInfo("SlackClientWrapper: received succesfull login response");
                ClientConnectOperation.SetResult(0);
            }
            else
            {
                //Logger.GetLogger().Info($"SlackClientWrapper: recevied failed login response, error is {loginResponse.error}");
                Logger.LogError($"SlackClientWrapper: recevied failed login response, error is {loginResponse.error}");
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
                    //Logger.GetLogger().Info("SlackClientWrapper: inside Dispose() - trying to close socket...");
                    Logger.LogInfo("SlackClientWrapper: inside Dispose() - trying to close socket...");
                    Client.CloseSocket();
                    cancelTask = true;
                    //Logger.GetLogger().Info("SlackClientWrapper: socket was closed");
                    Logger.LogInfo("SlackClientWrapper: socket was closed");
                }
                catch (Exception ex)
                {
                    //Logger.GetLogger().Info("Failed to gracefully shutdown Slack connection", ex);
                    Logger.LogError($"Failed to gracefully shutdown Slack connection {ex}");
                }
            }
            if (cancelTask && !ClientConnectOperation.Task.IsCompleted)
            {
                ClientConnectOperation.SetCanceled();
                //Logger.GetLogger().Info("SlackClientWrapper: subscription task was cancelled");
                Logger.LogWarning("SlackClientWrapper: subscription task was cancelled");
            }
        }
    }
}