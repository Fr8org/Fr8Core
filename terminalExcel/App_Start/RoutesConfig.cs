using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.StructureMap;
using StructureMap;
using PluginBase.BaseClasses;

namespace terminalExcel
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BasePluginWebApiConfig.Register(config);            

            config.Routes.MapHttpRoute(
                name: "PluginExcel",
                routeTemplate: "plugin_excel/{controller}/{id}"                
            );
        }
    }
}
