using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalFr8Core
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Built-In Services",
            IconPath = "/Content/icons/web_services/fr8-core-icon-64x64.png"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalFr8Core.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalFr8Core",
            Label = "Fr8Core",
            Version = "1"
        };
    }
}