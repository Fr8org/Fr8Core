using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Documentation.Swagger
{
    public class TerminalRegistrationSampleFactory : ISwaggerSampleFactory<TerminalRegistrationDTO>
    {
        public TerminalRegistrationDTO GetSampleData()
        {
            return new TerminalRegistrationDTO
            {
                Endpoint = "https://terminalfr8Core.fr8.co"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}