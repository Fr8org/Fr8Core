using PusherServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Configuration.Azure;

namespace Utilities
{
    public class PusherNotifier
    {
        private Pusher _pusher { get; set; }

        public PusherNotifier(string appId = null, string appKey = null, string appSecret = null)
        {
            if (!appId.IsNullOrEmpty() && !appKey.IsNullOrEmpty() && !appSecret.IsNullOrEmpty())
            {
                _pusher = new Pusher(appId, appKey, appSecret, new PusherOptions() { Encrypted = true });
            }
            else
            {
                _pusher = new Pusher(
                    CloudConfigurationManager.AppSettings.GetSetting("pusherAppId"),
                    CloudConfigurationManager.AppSettings.GetSetting("pusherAppKey"),
                    CloudConfigurationManager.AppSettings.GetSetting("pusherAppSecret"), 
                    new PusherOptions() { Encrypted = true }
                );
            }
        }

        public void Notify(string channelName, string eventName, object message)
        {
            var result = _pusher.Trigger(channelName, eventName, message);
        }
    }
}
