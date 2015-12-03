using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.Owin.Hosting;
using Owin;
using terminalGoogle.Controllers;
using TerminalBase.BaseClasses;

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
                    typeof(TerminalController),
                    typeof(EventController)
                };
            }
        }

        public class SelfHostStartup
        {
            public void Configuration(IAppBuilder app)
            {
                var config = new HttpConfiguration();
                BaseTerminalWebApiConfig.Register("Google", config);
                app.UseWebApi(config);
            }
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<SelfHostFactory.SelfHostStartup>(url: url);
        }
    }
}
