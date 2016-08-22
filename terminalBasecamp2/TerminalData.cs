using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalBasecamp2
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO ActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "Basecamp2",
            IconPath = "/Content/icons/web_services/basecamp2-icon-64x64.png",
            Type = "WebService"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalBasecamp2.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalBasecamp2",
            Label = "Basecamp2",
            Version = "1",
            AuthenticationType = AuthenticationType.External,
        };
    }
}