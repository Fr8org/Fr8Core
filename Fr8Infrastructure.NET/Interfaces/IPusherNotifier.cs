using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Interfaces
{
    public interface IPusherNotifier
    {
        void NotifyUser(NotificationMessageDTO notificationMessage, string userId);
    }
}