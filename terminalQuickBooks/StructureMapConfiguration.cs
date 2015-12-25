using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Services;
using StructureMap;
using StructureMap.Configuration.DSL;
using terminalQuickBooks.Interfaces;
using terminalQuickBooks.Services;

namespace terminalQuickBooks
{
    public class TerminalQuickBooksStructureMapConfiguration
    {
        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<IAction>().Use<Hub.Services.Action>();
                For<ITerminal>().Use<Terminal>();
                For<ICrateManager>().Use<CrateManager>();
                For<IRouteNode>().Use<RouteNode>();
                For<IJournalEntry>().Use<JournalEntry>();
                For<IQuickBooksIntegration>().Use<QuickBooksIntegration>();
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