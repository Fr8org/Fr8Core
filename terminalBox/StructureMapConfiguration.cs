using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.StructureMap;
using StructureMap;

namespace terminalBox
{
    public class BoxStructureMapBootstrapper
    {
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
