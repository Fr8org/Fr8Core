using StructureMap;
using StructureMap.Configuration.DSL;
using TerminalBase.Services;

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
            ObjectFactory.Configure(x => x.AddRegistry<LiveMode>());
        }

        public class LiveMode : Registry
        {
            public LiveMode()
            {                
                For<IHubCommunicator>().Use<DefaultHubCommunicator>();
            }            
        }
    }
}
