using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalStatX
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "StatX",
            IconPath = "/Content/icons/web_services/statx-logo.png"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalStatX.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalStatX",
            Label = "StatX",
            Version = "1",
            AuthenticationType = AuthenticationType.PhoneNumberWithCode
        };
    }
}