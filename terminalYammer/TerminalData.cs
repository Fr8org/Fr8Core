using Fr8Data.DataTransferObjects;
using Fr8Data.States;
using Utilities.Configuration.Azure;

namespace terminalYammer
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Yammer",
            IconPath = "/Content/icons/web_services/yammer-64x64.png"
        };
        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalYammer.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalYammer",
            Label = "Yammer",
            Version = "1",
            AuthenticationType = AuthenticationType.External
        };
    }
}