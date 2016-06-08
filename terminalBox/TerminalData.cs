using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalBox
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Box",
            IconPath = "/Content/icons/web_services/Box-logo_64x64.png"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO()
        {
            Name = "terminalBox",
            TerminalStatus = TerminalStatus.Active,
            Endpoint = CloudConfigurationManager.GetSetting("terminalBox.TerminalEndpoint"),
            Version = "1",
            AuthenticationType = AuthenticationType.External
        };
    }
}