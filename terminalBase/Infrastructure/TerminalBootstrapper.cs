using Moq;
using StructureMap;
using StructureMap.Configuration.DSL;

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
                For<IHubCommunicator>().Use<DefaultHubCommunicator>();
            }            
        }

        public class TestMode : Registry
        {
            public TestMode()
            {
                var hubCommunicator = new Mock<IHubCommunicator>();
                For<IHubCommunicator>().Use(hubCommunicator.Object);
            }
        }
    }
}
