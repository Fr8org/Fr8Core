using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using StructureMap;
using Hub.StructureMap;
using TerminalBase;
using TerminalBase.BaseClasses;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "TerminalDocuSign",
                routeTemplate: "terminal_docusign/{controller}/{id}"
            );
            config.Routes.MapHttpRoute(
    name: "TerminalDocuSignActionCatchAll",
    routeTemplate: "actions/{*actionType}",
    defaults: new { controller = "Action", action = "Execute" }); //It calls ActionController#Execute in an MVC style
        }
    }
}
