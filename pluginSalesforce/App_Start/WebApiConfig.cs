using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.StructureMap;
using StructureMap;

namespace pluginSalesforce
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "pluginSalesforce",
                routeTemplate: "plugin_salesforce/{controller}/{id}"
            );         
        }
    }
}
