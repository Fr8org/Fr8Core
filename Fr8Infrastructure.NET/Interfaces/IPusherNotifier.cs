using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Interfaces
{
    public interface IPusherNotifier
    {
        void Notify(string channelName, NotificationType eventName, object message);
        void NotifyUser(object message, NotificationType eventName, string userId);
    }
}