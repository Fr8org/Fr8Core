using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.States;
using fr8.Infrastructure.Utilities.Configuration;

namespace terminalTwilio
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Twilio",
            IconPath = "/Content/icons/web_services/twilio-icon-64x64.png"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Name = "terminalTwilio",
            Label = "Twilio",
            TerminalStatus = TerminalStatus.Active,
            Endpoint = CloudConfigurationManager.GetSetting("terminalTwilio.TerminalEndpoint"),
            Version = "1",
            AuthenticationType = AuthenticationType.None
        };
    }
}