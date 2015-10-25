using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.StructureMap;
using TerminalBase;
using terminalDocuSign.Infrastructure;
using TerminalBase.BaseClasses;
using StructureMap;

namespace terminalDocuSign
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BasePluginWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "PluginDocuSign",
                routeTemplate: "plugin_docusign/{controller}/{id}"
            );
        }
    }
}
