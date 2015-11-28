using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.Owin.Hosting;
using Owin;
using terminalTwilio.Controllers;
using TerminalBase.BaseClasses;

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
                    typeof(TerminalController)
                };
            }
        }

        public class SelfHostStartup
        {
            public void Configuration(IAppBuilder app)
            {
                var config = new HttpConfiguration();

                WebApiConfig.Register(config);

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
