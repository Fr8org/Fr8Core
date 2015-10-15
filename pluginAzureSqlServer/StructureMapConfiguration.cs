using System;
using System.Web.Http;
using Core.Interfaces;
using Core.Services;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap.Configuration.DSL;
using pluginAzureSqlServer.Infrastructure;
using StructureMap;

namespace pluginAzureSqlServer
{
    public class PluginAzureSqlServerStructureMapRegistries
    {
        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<IAction>().Use<Core.Services.Action>();
                For<IPlugin>().Use<Plugin>();
                For<ICrate>().Use<Crate>();
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
