using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using StructureMap;
using Hub.StructureMap;
using TerminalBase.BaseClasses;

namespace terminalExcel
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register(config);            

            config.Routes.MapHttpRoute(
                name: "TerminalExcel",
                routeTemplate: "terminal_excel/{controller}/{id}"                
            );
        }
    }
}
