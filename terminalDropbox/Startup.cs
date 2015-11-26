using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using TerminalBase;
using TerminalBase.BaseClasses;

[assembly: OwinStartup(typeof(terminalDropbox.Startup))]

namespace terminalDropbox
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            TerminalDropboxStructureMapBootstrapper.ConfigureDependencies(TerminalDropboxStructureMapBootstrapper.DependencyType.LIVE);
            WebApiConfig.Register(new HttpConfiguration());
            StartHosting("terminal_DropBox");
        }
    }
}
