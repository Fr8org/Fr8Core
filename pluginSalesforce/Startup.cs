using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using PluginBase;
using PluginBase.BaseClasses;
using pluginSalesforce;

[assembly: OwinStartup(typeof(pluginSalesforce.Startup))]

namespace pluginSalesforce
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            PluginSalesforceStructureMapBootstrapper.ConfigureDependencies(PluginSalesforceStructureMapBootstrapper.DependencyType.LIVE);

            Task.Run(() =>
            {
                BasePluginController curController = new BasePluginController();
                curController.AfterStartup("plugin_salesforce");
            });
        }
    }
}
