using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalDemo
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Demo Services"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalDemo.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalDemo",
            Label = "Fr8Demo",
            Version = "1"
        };
    }
}