using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using TerminalBase;
using TerminalBase.BaseClasses;
using System.Web.Http;
using TerminalBase.Infrastructure;
using System.Web.Http.Dispatcher;

[assembly: OwinStartup(typeof(terminalAtlassian.Startup))]

namespace terminalAtlassian
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            TerminalAtlassianStructureMapBootstrapper.ConfigureDependencies(TerminalAtlassianStructureMapBootstrapper.DependencyType.LIVE);

            ConfigureProject(selfHost, null);
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();

            app.UseWebApi(_configuration);

            if (!selfHost)
            {
                StartHosting("terminalAtlassian");
            }
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                typeof(Controllers.ActivityController),
                typeof(Controllers.TerminalController),
                typeof(Controllers.AuthenticationController)
            };
        }
    }
}
