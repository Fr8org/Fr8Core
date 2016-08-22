using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalSalesforce
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO ActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "Salesforce",
            IconPath = "/Content/icons/web_services/salesforce-icon-64x64.png",
            Type = "WebService"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO()
        {
            Name = "terminalSalesforce",
            Label = "Salesforce",
            TerminalStatus = TerminalStatus.Active,
            Endpoint = CloudConfigurationManager.GetSetting("terminalSalesforce.TerminalEndpoint"),
            Version = "1",
            AuthenticationType = AuthenticationType.External
        };
    }
}