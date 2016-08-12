using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalQuickBooks
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO ActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "QuickBooks",
            IconPath = "/Content/icons/web_services/quickbooks-icon-64x64.png",
            Type = "WebService"
        };
        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Name = "terminalQuickBooks",
            Label = "QuickBooks",
            TerminalStatus = TerminalStatus.Active,
            Endpoint = CloudConfigurationManager.GetSetting("terminalQuickBooks.TerminalEndpoint"),
            Version = "1",
            AuthenticationType = AuthenticationType.External
        };
    }
}