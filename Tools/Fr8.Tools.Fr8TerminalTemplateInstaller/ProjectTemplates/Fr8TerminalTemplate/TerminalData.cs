using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace $safeprojectname$
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "$projectname$",
            IconPath = "http://yourserver.com/icon.png"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("$safeprojectname$.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "$safeprojectname$",
            Label = "$projectname$",
            Version = "1"
        };
    }
}