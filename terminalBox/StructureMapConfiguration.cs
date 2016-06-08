using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.StructureMap;
using StructureMap;

namespace terminalBox
{
    public class BoxStructureMapBootstrapper
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
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }

    }
}
