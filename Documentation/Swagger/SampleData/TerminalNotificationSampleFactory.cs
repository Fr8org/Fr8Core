using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Documentation.Swagger
{
    public class TerminalNotificationSampleFactory : ISwaggerSampleFactory<TerminalNotificationDTO>
    {
        public TerminalNotificationDTO GetSampleData()
        {
            return new TerminalNotificationDTO
            {
                Type = "Success",
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