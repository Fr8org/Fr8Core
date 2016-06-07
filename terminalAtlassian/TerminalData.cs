using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.States;
using fr8.Infrastructure.Utilities.Configuration;

namespace terminalAtlassian
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Atlassian",
            IconPath = "/Content/icons/web_services/jira-icon-64x64.png"
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