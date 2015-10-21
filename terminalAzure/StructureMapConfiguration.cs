using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap;
using StructureMap.Configuration.DSL;
using Core.Interfaces;
using Core.Managers;
using Core.Services;
using terminalAzure.Infrastructure;

namespace terminalAzure
{
    public class PluginAzureSqlServerStructureMapRegistries
    {
        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<IAction>().Use<Core.Services.Action>();
                For<IPlugin>().Use<Plugin>();
                For<ICrateManager>().Use<CrateManager>();
                For<IDbProvider>().Use<SqlClientDbProvider>();
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
