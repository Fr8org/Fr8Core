using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.StructureMap;
using terminal_AzureSqlServer.Infrastructure;
using terminal_base;
using TerminalBase.BaseClasses;
using StructureMap;

namespace terminal_AzureSqlServer
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
           
          BaseTerminalWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "TerminalAzureSqlServer",
                routeTemplate: "terminal_azure_sql_server/{controller}/{id}"                
            );
        }
    }
}
