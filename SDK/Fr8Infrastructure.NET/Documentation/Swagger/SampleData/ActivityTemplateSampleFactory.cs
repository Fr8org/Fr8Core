using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class ActivityTemplateSampleFactory : ISwaggerSampleFactory<ActivityTemplateDTO>
    {
        private readonly ISwaggerSampleFactory<TerminalDTO> _terminalSampleFactory;

        public ActivityTemplateSampleFactory(ISwaggerSampleFactory<TerminalDTO> terminalSampleFactory)
        {
            _terminalSampleFactory = terminalSampleFactory;
        }

        public ActivityTemplateDTO GetSampleData()
        {
            return new ActivityTemplateDTO
            {
                Id = Guid.Parse("D64F823D-E127-41E5-A3E5-0D48BA2750DB"),
                Name = "Build_Message",
                Label = "Build a Message",
                Version = "1",
                MinPaneWidth = 330,
                Terminal = _terminalSampleFactory.GetSampleData(),
                Tags = string.Empty
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}