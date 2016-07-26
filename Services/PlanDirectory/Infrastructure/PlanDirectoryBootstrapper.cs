using System;
using System.IO;
using System.Reflection;
using System.Web.Hosting;
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
                For<IHubCommunicatorFactory>().Use(
                    x => new PlanDirectoryHubCommunicatorFactory(
                        ObjectFactory.GetInstance<IRestfulServiceClientFactory>(),
                        CloudConfigurationManager.GetSetting("HubApiBaseUrl"),
                        CloudConfigurationManager.GetSetting("PlanDirectorySecret")
                    )
                );
                var serverPath = GetServerPath();
                var planDirectoryUrl = new Uri(CloudConfigurationManager.GetSetting("PlanDirectoryUrl"));
                ConfigureManifestPageGenerator(planDirectoryUrl, serverPath);
            }

            private void ConfigureManifestPageGenerator(Uri planDirectoryUrl, string serverPath)
            {
                var templateGenerator = new TemplateGenerator(new Uri($"{planDirectoryUrl}/ManifestPages"), $"{serverPath}/ManifestPages");
                For<IManifestPageGenerator>().Use<ManifestPageGenerator>().Ctor<ITemplateGenerator>().Is(templateGenerator);
            }

            private static string GetServerPath()
            {
                var serverPath = HostingEnvironment.MapPath("~");
                if (serverPath == null)
                {
                    var uriPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
                    serverPath = new Uri(uriPath).LocalPath;
                }
                return serverPath;
            }
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}