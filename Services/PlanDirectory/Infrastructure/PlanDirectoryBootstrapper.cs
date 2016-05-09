using StructureMap;
using StructureMap.Configuration.DSL;

namespace PlanDirectory.Infrastructure
{
    public class PlanDirectoryBootStrapper
    {
        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<IPlanTemplate>().Use<PlanTemplate>();
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}