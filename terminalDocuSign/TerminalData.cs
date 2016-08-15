using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalDocuSign
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO ActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "DocuSign",
            IconPath = "/Content/icons/web_services/docusign-icon-64x64.png",
            Type = "WebService"
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