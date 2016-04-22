using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using TerminalBase.BaseClasses;

[assembly: OwinStartup(typeof(terminalTest.Startup))]
namespace terminalTest
{
    public class Startup: BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, null);
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();

            app.UseWebApi(_configuration);

            if (!selfHost)
            {
                StartHosting("terminalAzure");
            }
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActivityController),
                    typeof(Controllers.TerminalController)
                };
        }
    }
}
