using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap;
using StructureMap.Configuration.DSL;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Services;
using TerminalSqlUtilities;
using terminalAzure.Infrastructure;

namespace terminalAzure
{
    public class TerminalAzureSqlServerStructureMapRegistries
    {
        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<IAction>().Use<Hub.Services.Action>();
                For<ITerminal>().Use<Terminal>().Singleton();
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
