using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.Owin.Hosting;
using Owin;

namespace terminalQuickBooks
{
    public class SelfHostFactory
    {
        public class QuickBooksControllerTypeResolver : IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
            {
                return new Type[] {
                    typeof(Controllers.ActionController),
                    typeof(Controllers.AuthenticationController),
                    typeof(Controllers.EventController),
                    typeof(Controllers.TerminalController)
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
                WebApiConfig.Register(config);
                config.Services.Replace(
                    typeof(IHttpControllerTypeResolver),
                    new QuickBooksControllerTypeResolver()
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
