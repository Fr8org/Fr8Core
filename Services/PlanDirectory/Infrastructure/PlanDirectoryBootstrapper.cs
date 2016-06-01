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
                For<IAuthTokenManager>().Use<AuthTokenManager>();
                For<IPlanTemplate>().Use<PlanTemplate>();
                For<ISearchProvider>().Use<SearchProvider>();
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}