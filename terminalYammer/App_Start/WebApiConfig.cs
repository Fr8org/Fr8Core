using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;

namespace terminalYammer
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("Yammer", config);
        }
    }
}
