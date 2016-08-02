using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Interfaces
{
    public interface IPusherNotifier
    {
        // Used for notifications coming from Hub
        void NotifyUser(NotificationMessageDTO notificationMessage, string userId);
        // Used for notifications coming from Terminals
        void NotifyTerminalEvent(NotificationMessageDTO notificationMessage, string userId);
    }
}