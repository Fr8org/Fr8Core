using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap;
using StructureMap.Configuration.DSL;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Services;

namespace terminalExcel
{
    public class TerminalExcelStructureMapRegistries
    {
        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<IAction>().Use<Hub.Services.Action>();
                For<ITerminal>().Use<Terminal>();
                For<ICrateManager>().Use<CrateManager>();
                For<IRouteNode>().Use<RouteNode>();
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }

        public static void TestConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}
