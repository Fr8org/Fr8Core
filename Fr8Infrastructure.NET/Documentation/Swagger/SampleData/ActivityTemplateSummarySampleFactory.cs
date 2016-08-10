using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class ActivityTemplateSummarySampleFactory : ISwaggerSampleFactory<ActivityTemplateSummaryDTO>
    {
        public ActivityTemplateSummaryDTO GetSampleData()
        {
            return new ActivityTemplateSummaryDTO
            {
                Version = "1",
                Name = "Build_Message",
                TerminalName = "terminalFr8Core",
                TerminalVersion = "1"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}