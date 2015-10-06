using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.StructureMap;
using terminalBase;
using pluginDocuSign.Infrastructure;
using TerminalBase.BaseClasses;
using StructureMap;

namespace pluginDocuSign
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "PluginDocuSign",
                routeTemplate: "plugin_docusign/{controller}/{id}"
            );
        }
    }
}
