using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;
using Microsoft.Owin;
using Owin;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using DependencyType = Hub.StructureMap.StructureMapBootStrapper.DependencyType;

[assembly: OwinStartup(typeof(terminalQuickBooks.Startup))]

namespace terminalQuickBooks
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, TerminalQuickbooksBootstrapper.ConfigureLive);
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();

            app.UseWebApi(_configuration);

            if (!selfHost)
            {
                StartHosting("terminalQuickBooks");
            }
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActivityController),
                    typeof(Controllers.EventController),
                    typeof(Controllers.AuthenticationController),
                    typeof(Controllers.TerminalController)
                };
        }
    }
}
