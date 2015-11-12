<<<<<<< HEAD
﻿
=======
>>>>>>> dev
﻿using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.Owin.Hosting;
using Owin;

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
<<<<<<< HEAD
                    typeof(Controllers.TerminalController)
=======
                    typeof(Controllers.PluginController)
>>>>>>> dev
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
<<<<<<< HEAD
                    name: "TerminalDocuSign",
                    routeTemplate: "terminal_docusign/{controller}/{id}",
=======
                    name: "PluginDocuSign",
                    routeTemplate: "plugin_docusign/{controller}/{id}",
>>>>>>> dev
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
