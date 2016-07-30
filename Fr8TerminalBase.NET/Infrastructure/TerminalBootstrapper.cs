using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Utilities;
using Fr8.TerminalBase.Services;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace Fr8.TerminalBase.Infrastructure
{
    public class TerminalBootstrapper
    {
        public static void ConfigureTest()
        {
            ObjectFactory.Configure(x => x.AddRegistry<TestMode>());
        }

        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<IConfigRepository>().Use<ConfigRepository>().Singleton();
                For<ICrateManager>().Use<CrateManager>().Singleton();
                For<IActivityExecutor>().Use<ActivityExecutor>();
                For<IHubEventReporter>().Use<HubEventReporter>().Singleton();
                For<IHubDiscoveryService>().Use<HubDiscoveryService>().Singleton();
                For<IHubLoggerService>().Use<HubLoggerService>().Singleton();
                For<IRetryPolicy>().Use(() => new SimpleRetryPolicy(5, 2000));
            }            
        }

        public class TestMode : Registry
        {
            public TestMode()
            {
                For<IConfigRepository>().Use<MockedConfigRepository>();
                For<ICrateManager>().Use<CrateManager>().Singleton();
                For<IActivityExecutor>().Use<ActivityExecutor>();
                For<IHubEventReporter>().Use<HubEventReporter>().Singleton();
                For<IHubDiscoveryService>().Use<HubDiscoveryService>().Singleton();
                For<IHubLoggerService>().Use<HubLoggerService>().Singleton();
                For<IRetryPolicy>().Use<SingleRunRetryPolicy>();
            }
        }
    }
}
