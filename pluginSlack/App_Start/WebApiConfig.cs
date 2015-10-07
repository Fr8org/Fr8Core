using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.StructureMap;
using terminal_base;
using TerminalBase.BaseClasses;
using StructureMap;

namespace pluginSlack
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "PluginSlack",
                routeTemplate: "plugin_slack/{controller}/{id}"                
            );
        }
    }
}
