using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using StructureMap;
using Hub.StructureMap;
using TerminalBase;
using TerminalBase.BaseClasses;

namespace terminalSlack
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "TerminalSlack",
                routeTemplate: "terminal_slack/{controller}/{id}"                
            );
            config.Routes.MapHttpRoute(
    name: "TerminalSlackActionCatchAll",
    routeTemplate: "actions/{*actionType}",
    defaults: new { controller = "Action", action = "Execute" });
        }
    }
}
