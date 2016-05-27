using Fr8Data.DataTransferObjects;
using Fr8Data.States;
using Utilities.Configuration.Azure;

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