using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.StructureMap;
using terminalAzure.Infrastructure;
using PluginBase;
using PluginBase.BaseClasses;
using StructureMap;

namespace terminalAzure
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
           
          BasePluginWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "PluginAzureSqlServer",
                routeTemplate: "plugin_azure_sql_server/{controller}/{id}"                
            );
        }
    }
}
