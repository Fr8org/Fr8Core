using System;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;

namespace HubWeb.Documentation.Swagger.SampleData
{
    public class ActivityTemplateSampleFactory : ISwaggerSampleFactory<ActivityTemplateDTO>
    {
        private readonly ISwaggerSampleFactory<TerminalDTO> _terminalSampleFactory;
        private readonly ISwaggerSampleFactory<WebServiceDTO> _webServiceSampleFactory;
        public ActivityTemplateSampleFactory(ISwaggerSampleFactory<TerminalDTO> terminalSampleFactory, ISwaggerSampleFactory<WebServiceDTO> webServiceSampleFactory)
        {
            _terminalSampleFactory = terminalSampleFactory;
            _webServiceSampleFactory = webServiceSampleFactory;
        }

        private ActivityTemplateDTO _sample;
        public ActivityTemplateDTO GetSampleData()
        {
            return _sample ?? (_sample = new ActivityTemplateDTO
            {
                Id = Guid.Parse("D64F823D-E127-41E5-A3E5-0D48BA2750DB"),
                Name = "Build_Message",
                Label = "Build a Message",
                Category = ActivityCategory.Processors,
                Version = "1",
                MinPaneWidth = 330,
                WebService = _webServiceSampleFactory.GetSampleData(),
                Terminal = _terminalSampleFactory.GetSampleData(),
                Tags = string.Empty
            });
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}