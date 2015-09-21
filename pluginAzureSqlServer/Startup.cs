using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Core.StructureMap;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using PluginBase;
using PluginBase.BaseClasses;

[assembly: OwinStartup(typeof(pluginAzureSqlServer.Startup))]

namespace pluginAzureSqlServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // StructureMap Dependencies configuration 
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE);
            PluginAzureSqlServerStructureMapBootstrapper.ConfigureDependencies(PluginAzureSqlServerStructureMapBootstrapper.DependencyType.LIVE);

            Task.Run(() =>
            {
                BasePluginController curController = new BasePluginController();
                curController.AfterStartup("plugin_azure_sql_server");
            });
        }
    }
}
