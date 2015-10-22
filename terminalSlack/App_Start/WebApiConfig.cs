using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.StructureMap;
using TerminalBase;
using TerminalBase.BaseClasses;
using StructureMap;

namespace terminalSlack
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BasePluginWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "PluginSlack",
                routeTemplate: "plugin_slack/{controller}/{id}"                
            );
        }
    }
}
