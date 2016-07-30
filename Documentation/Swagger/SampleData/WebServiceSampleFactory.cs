using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Documentation.Swagger
{
    public class WebServiceSampleFactory : ISwaggerSampleFactory<WebServiceDTO>
    {
        public WebServiceDTO GetSampleData()
        {
            return new WebServiceDTO
            {
                Name = "Built-In Services",
                IconPath = "https://fr8.co/Content/img/site/site-logo.png"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}