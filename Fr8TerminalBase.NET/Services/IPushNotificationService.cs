using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.TerminalBase.Services
{
    public interface IPushNotificationService
    {
        Task PushUserNotification(ActivityTemplateDTO activityTemplate, NotificationArea notificationArea, string subject, string message);
    }
}
