using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class TerminalSampleFactory : ISwaggerSampleFactory<TerminalDTO>
    {
        public TerminalDTO GetSampleData()
        {
            return new TerminalDTO
            {
                Endpoint = "https://terminalfr8core.fr8.co",
                TerminalStatus = TerminalStatus.Active,
                Name = "terminalFr8Core",
                Label = "Fr8Core",
                Version = "1",
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}