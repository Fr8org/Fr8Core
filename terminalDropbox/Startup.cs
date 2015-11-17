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

[assembly: OwinStartup(typeof(terminalDropbox.Startup))]

namespace terminalDropbox
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            PluginDropboxStructureMapBootstrapper.ConfigureDependencies(PluginDropboxStructureMapBootstrapper.DependencyType.LIVE);

            StartHosting("terminal_dropbox");
        }
    }
}
