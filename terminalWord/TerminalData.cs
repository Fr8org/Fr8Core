using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalWord
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO ActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "Word",
            IconPath = "/Content/icons/web_services/ms-word-icon-64x64.png",
            Type = "WebService"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalWord.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalWord",
            Label = "Word",
            Version = "1"
        };
    }
}