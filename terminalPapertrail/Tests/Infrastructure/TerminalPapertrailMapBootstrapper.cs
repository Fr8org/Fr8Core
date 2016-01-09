using Moq;
using StructureMap;
using StructureMap.Configuration.DSL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminalPapertrail.Interfaces;
using terminalPapertrail.Services;
using DependencyType = Hub.StructureMap.StructureMapBootStrapper.DependencyType;

namespace terminalPapertrail.Tests.Infrastructure
{
    public class TerminalPapertrailMapBootstrapper
    {
        public static void ConfigureDependencies(DependencyType type)
        {
            switch (type)
            {
                case DependencyType.TEST:
                    ObjectFactory.Configure(x => x.AddRegistry<TestMode>()); // No test mode yet
                    break;
                case DependencyType.LIVE:
                    ObjectFactory.Configure(x => x.AddRegistry<LiveMode>());
                    break;
            }
        }

        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<IPapertrailLogger>().Use<PapertrailLogger>();
            }
        }

        public class TestMode : Registry
        {
            public TestMode()
            {
                Mock<IPapertrailLogger> papertrailLoggerMock = new Mock<IPapertrailLogger>(MockBehavior.Default);
                For<IPapertrailLogger>().Use(papertrailLoggerMock.Object);
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}