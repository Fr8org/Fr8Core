using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.Owin.Hosting;
using Owin;
using terminalGoogle.Controllers;

namespace terminalGoogle
{
    public class SelfHostFactory
    {
        public class DocuSignControllerTypeResolver : IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
            {
                return new[] {
                    typeof(ActionController),
                    typeof(PluginController)
                };
            }
        }

        public class SelfHostStartup
        {
            public void Configuration(IAppBuilder app)
            {
                var config = new HttpConfiguration();

                // Web API routes
                config.MapHttpAttributeRoutes();

                config.Routes.MapHttpRoute(
                    name: "TerminalGoogle",
                    routeTemplate: "terminal_google/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );

                config.Services.Replace(
                    typeof(IHttpControllerTypeResolver),
                    new DocuSignControllerTypeResolver()
                );

                app.UseWebApi(config);
            }
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<SelfHostFactory.SelfHostStartup>(url: url);
        }
    }
}
