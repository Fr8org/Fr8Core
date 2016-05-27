using Fr8Data.DataTransferObjects;
using Fr8Data.States;
using Utilities.Configuration.Azure;

namespace terminalGoogle
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Google",
            IconPath = "/Content/icons/web_services/google-icon-64x64.png"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalGoogle.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalGoogle",
            Label = "Google",
            Version = "1",
            AuthenticationType = AuthenticationType.External
        };
    }
}