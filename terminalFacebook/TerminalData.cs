using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalFacebook
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Facebook Activities",
            IconPath = "/Content/icons/web_services/facebook-icon-64x64.png"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalFacebook.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalFacebook",
            Label = "Facebook",
            Version = "1",
            AuthenticationType = AuthenticationType.External
        };
    }
}