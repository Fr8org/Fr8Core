using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class DetailedMessageSampleFactory : ISwaggerSampleFactory<DetailedMessageDTO>
    {
        public DetailedMessageDTO GetSampleData()
        {
            return new DetailedMessageDTO
            {
                Message = "Basic information",
                MessageDetails = "Detailed information"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}