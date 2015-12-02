using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using StructureMap;
using Hub.StructureMap;
using TerminalBase;
using TerminalBase.BaseClasses;

namespace terminalQuickBooks
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "TerminalQuickBooks",
                routeTemplate: "terminal_docusign/{controller}/{id}"
            );
        }
    }
}
