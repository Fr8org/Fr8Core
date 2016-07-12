using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;

namespace HubWeb.Documentation.Swagger
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
                PublicIdentifier = "2DB48191-CDA3-4922-9CC2-A636E828063F"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}