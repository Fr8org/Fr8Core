using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using PluginBase;

namespace pluginDockyardCore
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "PluginDockyardCoreServer",
                routeTemplate: "pluginDockyardCore/{controller}/{id}"
            );

            //Web API Exception Filter
            config.Filters.Add(new WebApiExceptionFilterAttribute());
        }
    }
}