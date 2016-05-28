using Fr8Data.Managers;
using StructureMap;
using StructureMap.Configuration.DSL;
using TerminalBase.Services;
using Utilities;

namespace TerminalBase.Infrastructure
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
