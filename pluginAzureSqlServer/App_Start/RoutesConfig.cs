using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.StructureMap;
using pluginAzureSqlServer.Infrastructure;
using terminal_base;
using TerminalBase.BaseClasses;
using StructureMap;

namespace pluginAzureSqlServer
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
           
          BaseTerminalWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "PluginAzureSqlServer",
                routeTemplate: "plugin_azure_sql_server/{controller}/{id}"                
            );
        }
    }
}
