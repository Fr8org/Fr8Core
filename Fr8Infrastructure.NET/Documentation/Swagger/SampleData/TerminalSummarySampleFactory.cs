using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class TerminalSummarySampleFactory : ISwaggerSampleFactory<TerminalSummaryDTO>
    {
        public TerminalSummaryDTO GetSampleData()
        {
            return new TerminalSummaryDTO
            {
                Version = "1",
                Name = "terminalFr8Core"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}