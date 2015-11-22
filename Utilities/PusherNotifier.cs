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
			_pusher = new Pusher(
				CloudConfigurationManager.AppSettings.GetSetting("pusherAppId"),
				CloudConfigurationManager.AppSettings.GetSetting("pusherAppKey"),
				CloudConfigurationManager.AppSettings.GetSetting("pusherAppSecret"),
				new PusherOptions
				{
					Encrypted = true
				}
			);
        }

        public void Notify(string channelName, string eventName, object message)
        {
            var result = _pusher.Trigger(channelName, eventName, message);
        }
    }
}
