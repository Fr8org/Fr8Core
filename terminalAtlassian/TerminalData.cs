using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalAtlassian
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO ActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "Atlassian",
            IconPath = "/Content/icons/web_services/jira-icon-64x64.png",
            Type = "WebService"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO()
        {
            Name = "terminalAtlassian",
            Label = "Atlassian",
            TerminalStatus = TerminalStatus.Active,
            Endpoint = CloudConfigurationManager.GetSetting("terminalAtlassian.TerminalEndpoint"),
            Version = "1",
            AuthenticationType = AuthenticationType.InternalWithDomain
        };
    }
}