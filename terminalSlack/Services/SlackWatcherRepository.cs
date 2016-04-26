using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SlackAPI;
using terminalSlack.Interfaces;
using Utilities.Logging;

namespace terminalSlack.Services
{
    public class SlackWatcherRepository : ISlackWatcherRepository
    {
        private readonly Dictionary<string, TaskCompletionSource<SlackSocketClient>> clientsByToken = new Dictionary<string, TaskCompletionSource<SlackSocketClient>>();

        private readonly Dictionary<string, HashSet<Guid>> subscriptionsByToken = new Dictionary<string, HashSet<Guid>>();

        private readonly Dictionary<Guid, string> tokensByActivity = new Dictionary<Guid, string>();
        
        private readonly object locker = new object();

        private int disposed;


        public Task Subscribe(string authToken, Guid activityId)
        {
            if (Interlocked.CompareExchange(ref disposed, 0, 0) == 1)
            {
                //It means that we disposed our registry already probably due to terminal shutdown
                return Task.FromResult(0);
            }
            lock (locker)
            {
                Unsubscribe(activityId);
                var client = new SlackSocketClient(authToken);
                var result = new TaskCompletionSource<SlackSocketClient>();
            }
        }

        public async Task Unsubscribe(Guid activityId)
        {
            string existingToken;
            if (tokensByActivity.TryGetValue(activityId, out existingToken))
            {
                tokensByActivity.Remove(activityId);
                var subscriptions = subscriptionsByToken[existingToken];
                subscriptions.Remove(activityId);
                //We've removed last subscription - disconnect from RTM websocket and remove it
                if (subscriptions.Count == 0)
                {
                    var client = clientsByToken[existingToken];
                    try
                    {
                        (await client.Task).CloseSocket();
                    }
                    catch (Exception ex)
                    {
                        Logger.GetLogger().Error("Failed to unsubscribe from Slack client events. Probably subscriptions was not completed prior to unsubscribe request", ex);
                    }
                    clientsByToken.Remove(existingToken);
                }
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 1)
            {
                //Already disposed
                return;
            }
            lock (locker)
            {
                foreach (var client in clientsByToken.Values.Select(x => x.Task.Result))
                {
                    client.CloseSocket();
                }
            }
        }
    }
}