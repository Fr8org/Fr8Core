using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalFacebook
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO ActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "Facebook",
            IconPath = "/Content/icons/web_services/facebook-icon-64x64.png",
            Type = "WebService"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalFacebook.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalFacebook",
            Label = "Facebook",
            Version = "1",
            AuthenticationType = AuthenticationType.External
        };
    }
}