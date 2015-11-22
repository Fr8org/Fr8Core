using System.Web.Http;
using TerminalBase.BaseClasses;

namespace terminalTwilio
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register(config);
            config.Routes.MapHttpRoute(
               name: "TerminalTwilio",
               routeTemplate: "terminal_twilio/{controller}/{id}"
           );
            config.Routes.MapHttpRoute(
name: "TerminalTwilioActionCatchAll",
routeTemplate: "actions/{*actionType}",
defaults: new { controller = "Action", action = "Execute" }); //It calls ActionController#Execute in an MVC style
        }
    }
}
