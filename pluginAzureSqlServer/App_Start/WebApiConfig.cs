using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.StructureMap;
using pluginAzureSqlServer.Infrastructure;
using StructureMap;

namespace pluginAzureSqlServer
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
           
          /*  ObjectFactory.Initialize(i =>
            {
                i.For<IDbProvider>().Use<SqlClientDbProvider>();
                
            });*/

            // Web API configuration and services
            new  Container().Inject<IDbProvider>(new SqlClientDbProvider());
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "PluginAzureSqlServer",
                routeTemplate: "plugin_azure_sql_server/{controller}/{id}"                
            );
        }
    }
}
