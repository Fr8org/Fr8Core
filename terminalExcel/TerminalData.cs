using Fr8Data.DataTransferObjects;
using Fr8Data.States;
using Utilities.Configuration.Azure;

namespace terminalExcel
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Excel"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalExcel.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalExcel",
            Label = "Excel",
            Version = "1"
        };
    }
}