using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Documentation.Swagger
{
    public class WebServiceSampleFactory : ISwaggerSampleFactory<WebServiceDTO>
    {
        private WebServiceDTO _sample;
        public WebServiceDTO GetSampleData()
        {
            return _sample ?? (_sample = new WebServiceDTO
            {
                Name = "Built-In Services",
                IconPath = "https://fr8.co/Content/img/site/site-logo.png"
            });
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}