using Fr8Data.Managers;
using Fr8Infrastructure.StructureMap;
using StructureMap;
using terminalAtlassian.Services;
using terminalAtlassian.Interfaces;
using TerminalBase.Infrastructure;
using TerminalBase.Services;

namespace terminalAtlassian
{
    public class TerminalAtlassianStructureMapBootstrapper
    {
        public enum DependencyType
        {
            TEST = 0,
            LIVE = 1
        }

        public static void ConfigureDependencies(DependencyType type)
        {
            switch (type)
            {
                case DependencyType.TEST:
                    ObjectFactory.Initialize(x => x.AddRegistry<LiveMode>()); // No test mode yet
                    break;
                case DependencyType.LIVE:
                    ObjectFactory.Initialize(x => x.AddRegistry<LiveMode>());
                    break;
            }
        }

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
