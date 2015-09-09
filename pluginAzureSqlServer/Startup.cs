using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            BasePluginController curController = new BasePluginController();
            curController.AfterStartup("plugin_azure_sql_server");
        }
    }
}
