using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalTelegram
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Telegram",
            IconPath = "/Content/icons/web_services/telegram-icon-64x64.png"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalTelegram.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalTelegram",
            Label = "Fr8Telegram",
            Version = "1",
            AuthenticationType = AuthenticationType.PhoneNumberWithCode
        };
    }
}