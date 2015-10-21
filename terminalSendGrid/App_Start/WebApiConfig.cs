using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Serialization;
using PluginBase.BaseClasses;

namespace terminalSendGrid
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BasePluginWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "PluginSendGridServer",
                routeTemplate: "pluginSendGrid/{controller}/{id}"
            );
        }
    }
}
