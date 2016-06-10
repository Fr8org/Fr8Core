using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.StructureMap;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
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
                For<ICrateManager>().Use<CrateManager>();
                For<IAtlassianService>().Use<AtlassianService>();
                For<IHubCommunicator>().Use<DefaultHubCommunicator>();
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}
