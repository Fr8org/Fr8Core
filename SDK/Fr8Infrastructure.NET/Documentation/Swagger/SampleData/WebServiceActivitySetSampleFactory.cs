using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class WebServiceActivitySetSampleFactory : ISwaggerSampleFactory<WebServiceActivitySetDTO>
    {
        private readonly ISwaggerSampleFactory<ActivityTemplateDTO> _activityTemplateSampleFactory;
        public WebServiceActivitySetSampleFactory(ISwaggerSampleFactory<ActivityTemplateDTO> activityTemplateSampleFactory)
        {
            _activityTemplateSampleFactory = activityTemplateSampleFactory;
        }

        public WebServiceActivitySetDTO GetSampleData()
        {
            return new WebServiceActivitySetDTO
            {
                Activities = new List<ActivityTemplateDTO> {  _activityTemplateSampleFactory.GetSampleData() },
                WebServiceIconPath = "https://fr8.co/Content/img/site/site-logo.png",
                WebServiceName = "Fr8 Core"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}