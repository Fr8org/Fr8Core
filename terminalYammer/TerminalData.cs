using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.States;
using fr8.Infrastructure.Utilities.Configuration;

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