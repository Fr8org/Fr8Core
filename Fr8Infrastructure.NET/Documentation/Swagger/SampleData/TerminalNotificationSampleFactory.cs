using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class TerminalNotificationSampleFactory : ISwaggerSampleFactory<NotificationMessageDTO>
    {
        public NotificationMessageDTO GetSampleData()
        {
            return new NotificationMessageDTO
            {
                NotificationType = NotificationType.GenericSuccess,
                Subject = "Good Message",
                Message = "Something good just happened",
                TerminalName = "terminalFr8Core",
                ActivityName = "Build_Message_v1",
                ActivityVersion = "1",
                Collapsed = false,
                TerminalVersion = "1"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}