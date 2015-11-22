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
            config.Routes.MapHttpRoute(
name: "TerminalPapertrailActionCatchAll",
routeTemplate: "actions/{*actionType}",
defaults: new { controller = "Action", action = "Execute" }); //It calls ActionController#Execute in an MVC style
        }
    }
}
