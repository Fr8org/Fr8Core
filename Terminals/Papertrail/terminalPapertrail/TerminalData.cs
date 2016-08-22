using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalPapertrail
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO ActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "Papertrail",
            IconPath = "/Content/icons/web_services/papertrail-icon-64x64.png",
            Type = "WebService"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Name = "terminalPapertrail",
            Label = "Papertrail",
            TerminalStatus = TerminalStatus.Active,
            Endpoint = CloudConfigurationManager.GetSetting("terminalPapertrail.TerminalEndpoint"),
            Version = "1"
        };
    }
}