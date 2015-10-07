using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using terminal_base;
using TerminalBase.BaseClasses;

namespace terminal_fr8Core
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "terminal_fr8CoreServer",
                routeTemplate: "terminal_fr8Core/{controller}/{id}"
            );
        }
    }
}