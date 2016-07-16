using Data.Repositories;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using Hub.Interfaces;
using Hub.Services;
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
                For<IPageGenerator>().Use<PageGenerator>();
                For<IPageDefinition>().Use<PageDefinition>();
                For<IPageDefinitionRepository>().Use<PageDefinitionRepository>();
                For<IHubCommunicator>().Use(
                    x => new DefaultHubCommunicator(
                        ObjectFactory.GetInstance<IRestfulServiceClient>(),
                        ObjectFactory.GetInstance<IHMACService>(),
                For<IHubCommunicatorFactory>().Use(
                    x => new PlanDirectoryHubCommunicatorFactory(
                        ObjectFactory.GetInstance<IRestfulServiceClientFactory>(),
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