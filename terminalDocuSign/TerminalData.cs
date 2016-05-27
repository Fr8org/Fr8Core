using Fr8Data.DataTransferObjects;
using Fr8Data.States;
using Utilities.Configuration.Azure;

namespace terminalDocuSign
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "DocuSign",
            IconPath = "/Content/icons/web_services/docusign-icon-64x64.png"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO()
        {
            Name = "terminalDocuSign",
            Label = "DocuSign",
            TerminalStatus = TerminalStatus.Active,
            Endpoint = CloudConfigurationManager.GetSetting("terminalDocuSign.TerminalEndpoint"),
            Version = "1",
            AuthenticationType = AuthenticationType.Internal
        };
    }
}