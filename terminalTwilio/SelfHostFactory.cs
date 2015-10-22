using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.Owin.Hosting;
using Owin;
using terminalTwilio.Controllers;

namespace terminalTwilio
{
    public class SelfHostFactory
    {
        private class TwilioControllerTypeResolver : IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
            {
                return new[] {
                    typeof(ActionController),
                    typeof(EventController),
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

                config.Services.Replace(typeof(IHttpControllerTypeResolver), new TwilioControllerTypeResolver());

                app.UseWebApi(config);
            }
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<SelfHostStartup>(url);
        }
    }
}
