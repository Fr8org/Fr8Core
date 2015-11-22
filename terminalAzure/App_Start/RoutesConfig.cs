using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using StructureMap;
using Hub.StructureMap;
using TerminalBase;
using TerminalBase.BaseClasses;
using terminalAzure.Infrastructure;

namespace terminalAzure
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
            config.Routes.MapHttpRoute(
    name: "TerminalAzureActionCatchAll",
    routeTemplate: "actions/{*actionType}",
    defaults: new { controller = "Action", action = "Execute" }); //It calls ActionController#Execute in an MVC style
        }
    }
}
