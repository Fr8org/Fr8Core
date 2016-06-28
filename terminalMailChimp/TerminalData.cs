using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalMailChimp
{
    public class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "MailChimp",
            IconPath = "/Content/icons/web_services/mailchimp-icon-64x64.png"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalMailChimp.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalMailChimp",
            Label = "MailChimp",
            Version = "1",
            AuthenticationType = AuthenticationType.External
        };
    }
}