using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Utilities;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace Fr8.TerminalBase.Infrastructure
{
    public class TerminalBootstrapper
    {
        public const string TestHubCommunicatorKey = "TestHubCommunicator";


        public static void ConfigureLive()
        {
            ObjectFactory.Configure(x => x.AddRegistry<LiveMode>());
        }

        public static void ConfigureTest()
        {
            ObjectFactory.Configure(x => x.AddRegistry<TestMode>());
        }

        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<IConfigRepository>().Use<ConfigRepository>();
                For<IHubCommunicator>().Use<DefaultHubCommunicator>();
                For<ICrateManager>().Use<CrateManager>();
                For<ActivityExecutor>().Use<ActivityExecutor>();
            }            
        }

        public class TestMode : Registry
        {
            public TestMode()
            {
                For<IConfigRepository>().Use<MockedConfigRepository>();
                For<IHubCommunicator>().Use<DefaultHubCommunicator>();
                For<ICrateManager>().Use<CrateManager>();
                For<ActivityExecutor>().Use<ActivityExecutor>();
            }
        }
    }
}
