using System;
using PusherServer;
using Utilities.Configuration.Azure;
using Utilities.Interfaces;

namespace Utilities
{
	public class PusherNotifier : IPusherNotifier
    {
        private Pusher _pusher { get; set; }

        public PusherNotifier()
        {
            var pusherAppId = CloudConfigurationManager.AppSettings.GetSetting("pusherAppId");
            var pusherAppKey = CloudConfigurationManager.AppSettings.GetSetting("pusherAppKey");
            var pusherAppSercret = CloudConfigurationManager.AppSettings.GetSetting("pusherAppSecret");

            if (!string.IsNullOrEmpty(pusherAppId) && !string.IsNullOrEmpty(pusherAppKey) &&
                !string.IsNullOrEmpty(pusherAppSercret))
            {
                _pusher = new Pusher(pusherAppId, pusherAppKey, pusherAppSercret,
                    new PusherOptions
                    {
                        Encrypted = true
                    }
                );
            }
            else
            {
                throw new Exception("Settings for pusher notifier not provided. Failed initializing Pusher.");
            }
        }

        public void Notify(string channelName, string eventName, object message)
        {
            _pusher?.Trigger(channelName, eventName, message);
        }

        public void NotifyUser(object message, string eventName, string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return;
            }

            var pusherChannel = BuildChannelName(userName);
            Notify(pusherChannel, eventName, message);
        }

        private string BuildChannelName(string email)
        {
            // If you change the way how channel name is constructed, be sure to change it also
            // in the client-side code (NotifierController.ts). 
            return "fr8pusher_" + Uri.EscapeUriString(email).Replace("%", "=");
        }
    }
}
