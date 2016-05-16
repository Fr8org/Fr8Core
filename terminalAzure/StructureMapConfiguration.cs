using System;
using System.Web.Http;
using Fr8Data.Managers;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap;
using StructureMap.Configuration.DSL;
using TerminalBase.BaseClasses;
using TerminalBase.Services;
using TerminalSqlUtilities;

namespace terminalAzure
{
    public class TerminalAzureSqlServerStructureMapRegistries
    {
        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<IDbProvider>().Use<SqlClientDbProvider>();
                
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
