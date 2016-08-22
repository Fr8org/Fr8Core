using System;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using PusherServer;

namespace Fr8.Infrastructure.Utilities
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

        public void NotifyUser(NotificationMessageDTO notificationMessage, string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var pusherChannel = BuildChannelName(userId);
                _pusher?.Trigger(pusherChannel, notificationMessage.NotificationType.ToString(), notificationMessage);
            }
        }

        private string BuildChannelName(string userId)
        {
            // If you change the way how channel name is constructed, be sure to change it also
            // in the client-side code (NotifierController.ts). 
            return "fr8pusher_" + Uri.EscapeUriString(userId).Replace("%", "=");
        }
    }
}
