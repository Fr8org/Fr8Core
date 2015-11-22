using System.Web.Http;
using TerminalBase.BaseClasses;

namespace terminalGoogle
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "TerminalGoogleServer",
                routeTemplate: "terminalGoogle/{controller}/{id}"
            );
            config.Routes.MapHttpRoute(
name: "TerminalGoogleActionCatchAll",
routeTemplate: "actions/{*actionType}",
defaults: new { controller = "Action", action = "Execute" }); //It calls ActionController#Execute in an MVC style
        }
    }
}