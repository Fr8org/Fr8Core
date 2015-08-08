using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using PluginUtilities;

[assembly: OwinStartup(typeof(pluginAzureSqlServer.Startup))]

namespace pluginAzureSqlServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            var data =
                new
                {
                    ObjectId = "azure_sql_plugin_object",
                    CustomerId = "not_applicable",
                    Data = "service_start_up",
                    PrimaryCategory = "Operations",
                    SecondaryCategory = "System Startup",
                    Activity = "system startup"
                };

            var json = new {Source = "azure_sql_plugin", EventType = "Plugin Incident", Data = data};
            PluginBase.AfterStartup(JsonConvert.SerializeObject(json));

            
        }
    }
}
