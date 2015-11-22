using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.Owin.Hosting;
using Owin;

namespace terminalExcel
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

                var startup = new StartupTerminalExcel();

                config.Routes.MapHttpRoute(
           name: "TerminalDropboxActionCatchAll",
           routeTemplate: "actions/{*actionType}",
           defaults: new { controller = "Action", action = "Execute" }); //It calls ActionController#Execute in an MVC style
                startup.Configuration(app, selfHost: true);

            }
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<SelfHostFactory.SelfHostStartup>(url: url);
        }
    }
}
