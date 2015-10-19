using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.StructureMap;
using PluginBase;
using pluginDocuSign.Infrastructure;
using PluginBase.BaseClasses;
using StructureMap;

namespace pluginDocuSign
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
