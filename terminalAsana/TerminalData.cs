using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalAsana
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO ActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "Asana",
            IconPath = "/Content/icons/web_services/asana-icon-64x64.png",
            Type = "WebService"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalAsana.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            AuthenticationType = AuthenticationType.External,
            Name = "terminalAsana",
            Label = "Asana",
            Version = "1"
        };
    }
}