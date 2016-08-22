using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class ResponseMessageSampleFactory : ISwaggerSampleFactory<ResponseMessageDTO>
    {
        public ResponseMessageDTO GetSampleData()
        {
            return new ResponseMessageDTO("Success")
            {
                Message = "Terminal activities were successfully discovered",
                CurrentActivity = "",
                CurrentTerminal = "terminalFr8Core",
                Details = string.Empty,
                ErrorCode = string.Empty
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}