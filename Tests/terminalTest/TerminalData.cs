using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.States;
using fr8.Infrastructure.Utilities.Configuration;

namespace terminalTest
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Terminal for debugging"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalTest.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalTest",
            Version = "1"
        };
    }
}