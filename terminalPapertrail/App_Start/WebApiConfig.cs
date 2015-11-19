using System.Web.Http;
using TerminalBase.BaseClasses;

namespace terminalPapertrail
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "TerminalPapertrail",
                routeTemplate: "terminal_papertrail/{controller}/{id}"
            );
        }
    }
}
