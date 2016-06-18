using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;

namespace Fr8.TerminalBase.Services
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly IHubCommunicator _hubCommunicator;

        public PushNotificationService(IHubCommunicator hubCommunicator)
        {
            _hubCommunicator = hubCommunicator;
        }
        
        public async Task PushUserNotification(ActivityTemplateDTO activityTemplate, string type, string subject, string message)
        {
            var notificationMsg = new TerminalNotificationDTO
            {
                Type = type,
                Subject = subject,
                Message = message,
                ActivityName = activityTemplate.Name,
                ActivityVersion = activityTemplate.Version,
                TerminalName = activityTemplate.Terminal.Name,
                TerminalVersion = activityTemplate.Terminal.Version,
                Collapsed = false
            };

            await _hubCommunicator.NotifyUser(notificationMsg);
        }
    }
}