namespace Utilities.Interfaces
{
	public interface IPusherNotifier
	{
		void Notify(string channelName, string eventName, object message);
        void Notify()
	}
}