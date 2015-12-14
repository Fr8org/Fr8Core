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
    }
}
