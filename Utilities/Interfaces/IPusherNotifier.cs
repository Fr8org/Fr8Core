namespace Utilities.Interfaces
{
	public interface IPusherNotifier
	{
		void Notify(string channelName, string eventName, object message);
        void NotifyUser(object message, string eventName, string userName);
	}
}