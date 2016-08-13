using Fr8.Infrastructure.StructureMap;
using StructureMap;
using terminalAtlassian.Services;
using terminalAtlassian.Interfaces;

namespace terminalAtlassian
{
    public class TerminalAtlassianStructureMapBootstrapper
    {
        public class LiveMode : StructureMapBootStrapper.LiveMode
        {
            public LiveMode()
            {
                For<IAtlassianService>().Use<AtlassianService>();
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
            configuration.For<IAtlassianEventManager>().Use<AtlassianEventManager>();
            configuration.For<IAtlassianSubscriptionManager>().Use<AtlassianSubscriptionManager>();
        }
    }
}
