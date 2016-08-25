using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalInstagram
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO ActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "Instagram",
            IconPath = "/Content/icons/web_services/instagram-icon-64x64.png",
            Type = "WebService"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalInstagram.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalInstagram",
            Label = "Instagram",
            Version = "1",
            AuthenticationType = AuthenticationType.External
        };
    }
}