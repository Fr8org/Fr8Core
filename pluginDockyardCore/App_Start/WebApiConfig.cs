using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using PluginBase;
using PluginBase.BaseClasses;

namespace pluginDockyardCore
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BasePluginWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "PluginDockyardCoreServer",
                routeTemplate: "pluginDockyardCore/{controller}/{id}"
            );
        }
    }
}