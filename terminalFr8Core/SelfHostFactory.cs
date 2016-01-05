using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.Owin.Hosting;
using Owin;
using TerminalBase.BaseClasses;

namespace terminalFr8Core
{
    public class SelfHostFactory
    {
        public class DocuSignControllerTypeResolver : IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
            {
                return new Type[] {
                    typeof(Controllers.ActionController),
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
                WebApiConfig.Register(config);
                app.UseWebApi(config);
            }
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<SelfHostFactory.SelfHostStartup>(url: url);
        }
    }
}
