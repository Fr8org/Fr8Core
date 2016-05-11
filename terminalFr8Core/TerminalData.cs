using Fr8Data.DataTransferObjects;
using Fr8Data.States;
using Utilities.Configuration.Azure;

namespace terminalFr8Core
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Built-In Services"
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