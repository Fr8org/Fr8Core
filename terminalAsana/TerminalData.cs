using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalAsana
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Asana",
            IconPath = "http://d1gwm4cf8hecp4.cloudfront.net/images/favicons/favicon.ico"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalAsana.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalAsana",
            Label = "Asana",
            Version = "1"
        };
    }
}