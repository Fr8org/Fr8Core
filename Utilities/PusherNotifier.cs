using PusherServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class PusherNotifier
    {
        private Pusher _pusher { get; set; }

        const string defaultAppId = "148580";
        const string defaultAppKey = "123dd339500fed0ddd78";
        const string defaultAppSecret = "598b1fdcdf903325d520";

        public PusherNotifier(string appId = null, string appKey = null, string appSecret = null)
        {
            if (!appId.IsNullOrEmpty() && !appKey.IsNullOrEmpty() && !appSecret.IsNullOrEmpty())
            {
                _pusher = new Pusher(appId, appKey, appSecret, new PusherOptions() { Encrypted = true });
            }
            else
            {
                _pusher = new Pusher(defaultAppId, defaultAppKey, defaultAppSecret, new PusherOptions() { Encrypted = true });
            }
        }

        public void Notify(string channelName, string eventName, object message)
        {
            var result = _pusher.Trigger(channelName, eventName, message);
        }
    }
}
