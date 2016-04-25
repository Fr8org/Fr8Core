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
            TerminalAtlassianStructureMapBootstrapper.ConfigureDependencies(TerminalAtlassianStructureMapBootstrapper.DependencyType.LIVE);
            WebApiConfig.Register(new HttpConfiguration());
            TerminalBootstrapper.ConfigureLive();
            StartHosting("terminal_Atlassian");
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
