using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Configuration.Azure;
using PlanDirectory.Interfaces;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace PlanDirectory.Infrastructure
{
    public class PlanTemplate : IPlanTemplate
    {
        private const string PlanTemplateIndexName = "PlanTemplateIndex";


        public async Task Initialize()
        {
            using (var searchClient = CreateAzureSearchClient())
            {
                await EnsurePlanTemplateIndexExists(searchClient);
            }
        }

        public async Task<IEnumerable<SearchPlanTemplateDTO>> Search(string text)
        {
            using (var searchClient = CreateAzureSearchClient())
            {
                var indexClient = searchClient.Indexes.GetClient(PlanTemplateIndexName);

                var searchResult = await indexClient.Documents.SearchAsync<SearchPlanTemplateDTO>(text);
                var resultingDocuments = searchResult.Results.Select(x => x.Document).ToList();

                return resultingDocuments;
            }
        }

        public async Task Publish(PublishPlanTemplateDTO planTemplate)
        {
            using (var searchClient = CreateAzureSearchClient())
            {
                var indexClient = searchClient.Indexes.GetClient(PlanTemplateIndexName);

                var document = ConvertToSearchPlanTemplate(planTemplate);

                var batch = IndexBatch.New(
                    new IndexAction<SearchPlanTemplateDTO>[]
                    {
                        IndexAction.Upload(document)
                    }
                );
                await indexClient.Documents.IndexAsync(batch);
            }
        }

        public async Task Unpublish(PublishPlanTemplateDTO planTemplate)
        {
            using (var searchClient = CreateAzureSearchClient())
            {
                var indexClient = searchClient.Indexes.GetClient(PlanTemplateIndexName);

                var document = ConvertToSearchPlanTemplate(planTemplate);

                var batch = IndexBatch.New(
                    new IndexAction<SearchPlanTemplateDTO>[]
                    {
                        IndexAction.Delete(document)
                    }
                );
                await indexClient.Documents.IndexAsync(batch);
            }
        }

        private string GetAzureSearchServiceName()
        {
            var serviceName = CloudConfigurationManager.GetSetting("AzureSearchServiceName");
            return serviceName;
        }

        private string GetAzureSearchApiKey()
        {
            var apiKey = CloudConfigurationManager.GetSetting("AzureSearchApiKey");
            return apiKey;
        }

        private SearchServiceClient CreateAzureSearchClient()
        {
            var searchClient = new SearchServiceClient(
                GetAzureSearchServiceName(),
                new SearchCredentials(GetAzureSearchApiKey())
            );

            return searchClient;
        }

        private Index CreateIndexDefinition()
        {
            var definition = new Index()
            {
                Name = PlanTemplateIndexName,
                Fields = new[]
                {
                    new Field("planTemplateId", DataType.Int32) { IsKey = true },
                    new Field("name", DataType.String),
                    new Field("description", DataType.String)
                }
            };

            return definition;
        }

        private SearchPlanTemplateDTO ConvertToSearchPlanTemplate(PublishPlanTemplateDTO dto)
        {
            var searchPlanTemplateDTO = new SearchPlanTemplateDTO()
            {
                PlanTemplateId = dto.PlanTemplateId,
                Name = dto.Name,
                Description = dto.Description
            };

            return searchPlanTemplateDTO;
        }

        private async Task EnsurePlanTemplateIndexExists(SearchServiceClient searchClient)
        {
            var exists = await searchClient.Indexes.ExistsAsync(PlanTemplateIndexName);
            if (!exists)
            {
                await searchClient.Indexes.CreateAsync(CreateIndexDefinition());
            }
        }
    }
}