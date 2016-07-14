using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Interfaces
{
    public interface IPusherNotifier
    {
        void Notify(string channelName, NotificationType notificationType, object message);
        void NotifyUser(object message, NotificationType notificationType, string userId);
    }
}