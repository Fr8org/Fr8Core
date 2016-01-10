using System.Web.Http;
using TerminalBase.BaseClasses;

namespace terminalTwilio
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("Twilio", config);

        }
    }
}
