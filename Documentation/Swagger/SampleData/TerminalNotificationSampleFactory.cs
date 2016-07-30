using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Documentation.Swagger
{
    public class TerminalNotificationSampleFactory : ISwaggerSampleFactory<NotificationMessageDTO>
    {
        public NotificationMessageDTO GetSampleData()
        {
            return new NotificationMessageDTO
            {
                NotificationType = NotificationType.GenericSuccess,
                NotificationArea = NotificationArea.ActivityStream,
                Message = "Something good just happened",
                TerminalName = "terminalFr8Core",
                ActivityName = "Build_Message_v1",
                ActivityVersion = "1",
                Collapsed = false,
                Subject = "Good Message",
                TerminalVersion = "1"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}