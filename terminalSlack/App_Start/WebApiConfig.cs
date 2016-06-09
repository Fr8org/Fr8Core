using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;

namespace terminalSlack
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("Slack", config);
        }
    }
}
