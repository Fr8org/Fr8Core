using System;
using System.IO;
using System.Reflection;
using System.Web.Hosting;
using Data.Repositories;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Interfaces;
using Hub.Services;
using HubWeb.Infrastructure_PD.Infrastructure;
using HubWeb.Infrastructure_PD.Interfaces;
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
                For<IFr8Account>().Use<Fr8Account>().Singleton();
                //For<IAuthTokenManager>().Use<AuthTokenManager>().Singleton();
                For<IPlanTemplate>().Use<PlanTemplate>().Singleton();
                For<ISearchProvider>().Use<SearchProvider>();
                For<ITagGenerator>().Use<TagGenerator>().Singleton();
                For<IPageDefinition>().Use<PageDefinition>().Singleton();
                For<IPageDefinitionRepository>().Use<PageDefinitionRepository>().Singleton();
                //For<IHubCommunicatorFactory>().Use(
                //    x => new PlanDirectoryHubCommunicatorFactory(
                //        ObjectFactory.GetInstance<IRestfulServiceClientFactory>(),
                //        CloudConfigurationManager.GetSetting("HubApiUrl"),
                //        CloudConfigurationManager.GetSetting("PlanDirectorySecret")
                //    )
                //);
                var serverPath = GetServerPath();

                //now it is hub
                var protocol = CloudConfigurationManager.GetSetting("ServerProtocol");
                var server = CloudConfigurationManager.GetSetting("ServerDomainName");
                var port = CloudConfigurationManager.GetSetting("ServerPort");
                var planDirectoryUrl = new Uri(protocol + server + port);
                //var planDirectoryUrl = new Uri(CloudConfigurationManager.GetSetting("PlanDirectoryUrl"));

                ConfigureManifestPageGenerator(planDirectoryUrl, serverPath);
                ConfigurePlanPageGenerator(planDirectoryUrl, serverPath);
                ConfigurePlanTemplateDetailsPageGenerator(planDirectoryUrl, serverPath);
            }

            private void ConfigurePlanPageGenerator(Uri planDirectoryUrl, string serverPath)
            {
                var templateGenerator = new TemplateGenerator(new Uri($"{planDirectoryUrl}categorypages"), $"{serverPath}/categorypages");
                For<IWebservicesPageGenerator>().Use<WebservicesPageGenerator>().Singleton().Ctor<ITemplateGenerator>().Is(templateGenerator);
            }

            private void ConfigureManifestPageGenerator(Uri planDirectoryUrl, string serverPath)
            {
                var templateGenerator = new TemplateGenerator(new Uri($"{planDirectoryUrl}manifestpages"), $"{serverPath}/manifestpages");
                For<IManifestPageGenerator>().Use<ManifestPageGenerator>().Singleton().Ctor<ITemplateGenerator>().Is(templateGenerator);
            }

            private void ConfigurePlanTemplateDetailsPageGenerator(Uri planDirectoryUrl, string serverPath)
            {
                var templateGenerator = new TemplateGenerator(new Uri($"{planDirectoryUrl}details"), $"{serverPath}/details");
                For<IPlanTemplateDetailsGenerator>().Use<PlanTemplateDetailsGenerator>().Singleton().Ctor<ITemplateGenerator>().Is(templateGenerator);
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