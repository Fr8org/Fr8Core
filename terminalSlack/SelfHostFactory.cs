using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;

namespace terminalSlack
{
    public class SelfHostFactory
    {
        public class SelfHostStartup
        {
            public void Configuration(IAppBuilder app)
            {
                var config = new HttpConfiguration();

                // Web API routes
                config.MapHttpAttributeRoutes();

                config.Routes.MapHttpRoute(
                    name: "TerminalSlack",
                    routeTemplate: "terminal_slack/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
                config.Routes.MapHttpRoute(
name: "TerminalSlackActionCatchAll",
routeTemplate: "actions/{*actionType}",
defaults: new { controller = "Action", action = "Execute" }); //It calls ActionController#Execute in an MVC style
                app.UseWebApi(config);
            }
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<SelfHostFactory.SelfHostStartup>(url: url);
        }
    }
}
