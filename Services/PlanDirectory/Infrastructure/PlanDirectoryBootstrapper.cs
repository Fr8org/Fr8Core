using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using PlanDirectory.Interfaces;
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
                For<ITagGenerator>().Use<TagGenerator>();
                For<IHubCommunicator>().Use(
                    x => new PlanDirectoryHubCommunicator(
                        ObjectFactory.GetInstance<IRestfulServiceClient>(),
                        CloudConfigurationManager.GetSetting("HubApiBaseUrl"),
                        CloudConfigurationManager.GetSetting("PlanDirectorySecret")
                    )
                );
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}