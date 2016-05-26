using Fr8Data.DataTransferObjects;
using Fr8Data.States;
using Utilities.Configuration.Azure;

namespace terminalPapertrail
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Papertrail",
            IconPath = "/Content/icons/web_services/papertrail-icon-64x64.png"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Name = "terminalPapertrail",
            Label = "Papertrail",
            TerminalStatus = TerminalStatus.Active,
            Endpoint = CloudConfigurationManager.GetSetting("terminalPapertrail.TerminalEndpoint"),
            Version = "1"
        };
    }
}