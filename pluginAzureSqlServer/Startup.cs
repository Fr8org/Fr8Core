using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Owin;
using PluginUtilities;

[assembly: OwinStartup(typeof(pluginAzureSqlServer.Startup))]

namespace pluginAzureSqlServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            PluginBase.AfterStartup(
                @"{""Source"" : ""azure_sql_plugin"",""EventType"" : ""Plugin Incident"",""Data"" : ""json list of key value pairs""}");
        }
    }
}
