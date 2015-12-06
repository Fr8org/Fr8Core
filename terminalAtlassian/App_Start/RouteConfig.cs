using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using TerminalBase.BaseClasses;

namespace terminalAtlassian
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            BaseTerminalWebApiConfig.RegisterRoutes("Atlassian", routes);
        }
    }
}
