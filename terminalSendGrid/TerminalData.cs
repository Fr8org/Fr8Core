using Fr8Data.DataTransferObjects;
using Fr8Data.States;
using Utilities.Configuration.Azure;

namespace terminalSendGrid
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "SendGrid"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Name = "terminalSendGrid",
            Label = "SendGrid",
            TerminalStatus = TerminalStatus.Active,
            Endpoint = CloudConfigurationManager.GetSetting("terminalSendGrid.TerminalEndpoint"),
            Version = "1"
        };
    }
}