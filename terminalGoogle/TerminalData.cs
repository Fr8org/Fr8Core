using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;


namespace terminalGoogle
{
    public static class TerminalData
    {
        // TODO: FR-4943, remove this.
        // public static WebServiceDTO GooogleWebServiceDTO = new WebServiceDTO
        // {
        //     Name = "Google",
        //     IconPath = "/Content/icons/web_services/google-icon-64x64.png"
        // };

        public static ActivityCategoryDTO GooogleActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "Google",
            IconPath = "/Content/icons/web_services/google-icon-64x64.png"
        };

        // TODO: FR-4943, remove this.
        // public static WebServiceDTO GmailWebServiceDTO = new WebServiceDTO
        // {
        //     Name = "Gmail",
        //     IconPath = "/Content/icons/web_services/gmail-icon-64x64.png"
        // };

        public static ActivityCategoryDTO GmailActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "Gmail",
            IconPath = "/Content/icons/web_services/gmail-icon-64x64.png"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalGoogle.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalGoogle",
            Label = "Google",
            Version = "1",
            AuthenticationType = AuthenticationType.External
        };
    }
}