﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using StructureMap;
using Hub.StructureMap;

namespace terminalSalesforce
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "terminalSalesforce",
                routeTemplate: "terminal_salesforce/{controller}/{id}"
            );
            config.Routes.MapHttpRoute(
name: "TerminalSalesforceActionCatchAll",
routeTemplate: "actions/{*actionType}",
defaults: new { controller = "Action", action = "Execute" }); //It calls ActionController#Execute in an MVC style
        }
    }
}
