using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap.Configuration.DSL;
using StructureMap;
using Core.Interfaces;
using Core.Managers;
using Core.Services;

namespace pluginExcel
{
    public class PluginExcelStructureMapRegistries
    {
        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<IAction>().Use<Core.Services.Action>();
                For<IPlugin>().Use<Plugin>();
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
