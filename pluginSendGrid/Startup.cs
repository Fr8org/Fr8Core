using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using System.Threading.Tasks;
using PluginBase.BaseClasses;

[assembly: OwinStartup(typeof(pluginSendGrid.Startup))]

namespace pluginSendGrid
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            PluginSendGridStructureMapBootstrapper.ConfigureDependencies(PluginSendGridStructureMapBootstrapper.DependencyType.LIVE);
        }
    }
}
