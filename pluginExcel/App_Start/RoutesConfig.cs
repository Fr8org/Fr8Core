using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.StructureMap;
using StructureMap;

namespace pluginExcel
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "PluginExcel",
                routeTemplate: "plugin_excel/{controller}/{id}"                
            );
        }
    }
}
