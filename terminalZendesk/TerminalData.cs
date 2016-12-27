using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalZendesk
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO ActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "Zendesk",
            IconPath = "/Content/icons/web_services/zendesk-icon-64x64.png",
            Type = "WebService"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Name = "terminalZendesk",
            Label = "Zendesk",
            TerminalStatus = TerminalStatus.Active,
            Endpoint = CloudConfigurationManager.GetSetting("terminalZendesk.TerminalEndpoint"),
            ProdUrl = "https://terminalZendesk.fr8.co",
            Version = "1",
            AuthenticationType = AuthenticationType.External
        };
    }
}