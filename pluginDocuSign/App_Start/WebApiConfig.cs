using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.StructureMap;
using PluginBase;
using pluginDocuSign.Infrastructure;
using StructureMap;

namespace pluginDocuSign
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "PluginDocuSign",
                routeTemplate: "plugin_docusign/{controller}/{id}"
            );

            //Web API Exception Filter
            config.Filters.Add(new WebApiExceptionFilterAttribute());

            //config.Routes.MapHttpRoute(
            //    name: "PluginDocuSign",
            //    routeTemplate: "plugin_docusign/actions/{action}"
            //);
        }
    }
}
