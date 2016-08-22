using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalYammer
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO ActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "Yammer",
            IconPath = "/Content/icons/web_services/yammer-64x64.png",
            Type = "WebService"
        };
        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalYammer.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalYammer",
            Label = "Yammer",
            Version = "1",
            AuthenticationType = AuthenticationType.External
        };
    }
}