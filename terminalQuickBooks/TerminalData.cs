using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.States;
using fr8.Infrastructure.Utilities.Configuration;

namespace terminalQuickBooks
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "QuickBooks",
            IconPath = "/Content/icons/web_services/quickbooks-icon-64x64.png"
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