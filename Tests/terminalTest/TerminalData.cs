using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalTest
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO WebServiceDTO = new ActivityCategoryDTO
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