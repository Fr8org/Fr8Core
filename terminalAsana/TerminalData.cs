using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalDemo
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Asana Activities",
            IconPath = "/Content/icons/web_services/fr8-demo-icon-64x64.png"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalAsana.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalAsana",
            Label = "Asana terminal",
            Version = "1"
        };
    }
}