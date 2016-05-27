using System.Web.Http;
using TerminalBase.BaseClasses;

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
