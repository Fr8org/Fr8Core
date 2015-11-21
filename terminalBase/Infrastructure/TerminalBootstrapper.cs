using StructureMap;
using StructureMap.Configuration.DSL;

namespace TerminalBase.Infrastructure
{
    public class TerminalBootstrapper
    {
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
