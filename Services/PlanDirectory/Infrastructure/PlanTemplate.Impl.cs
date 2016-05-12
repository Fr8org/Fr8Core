using System;
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
        private const string PlanTemplateIndexName = "plan-template-index";


        public async Task Initialize()
        {
            using (var searchClient = CreateAzureSearchClient())
            {
                // await DeletePlanTemplateIndexIfExists(searchClient);
                await EnsurePlanTemplateIndexExists(searchClient);
            }
        }

        public async Task<SearchResultDTO> Search(SearchRequestDTO request)
        {
            using (var searchClient = CreateAzureSearchClient())
            {
                using (var indexClient = searchClient.Indexes.GetClient(PlanTemplateIndexName))
                {
                    var sp = new SearchParameters();
                    sp.OrderBy = new List<string>() { "name" };
                    sp.IncludeTotalResultCount = true;

                    if (request.PageStart > 0 && request.PageSize > 0)
                    {
                        sp.Skip = (request.PageStart - 1) * request.PageSize;
                        sp.Top = request.PageSize;
                    }

                    var text = string.IsNullOrEmpty(request.Text) ? "*" : request.Text;

                    var searchResult = await indexClient.Documents.SearchAsync(text, sp);
                    var resultingDocuments = searchResult.Results.Select(x => ConvertSearchDocumentToDto(x.Document)).ToList();

                    var dto = new SearchResultDTO()
                    {
                        PlanTemplates = resultingDocuments,
                        TotalCount = searchResult.Count.GetValueOrDefault()
                    };

                    return dto;
                }
            }
        }

        public async Task Publish(PublishPlanTemplateDTO planTemplate)
        {
            using (var searchClient = CreateAzureSearchClient())
            {
                using (var indexClient = searchClient.Indexes.GetClient(PlanTemplateIndexName))
                {

                    var document = ConvertToSearchDocument(planTemplate);

                    var batch = IndexBatch.New(
                        new IndexAction[]
                        {
                            IndexAction.Upload(document)
                        }
                    );

                    var indexResult = await indexClient.Documents.IndexAsync(batch);
                }
            }
        }

        public async Task Unpublish(PublishPlanTemplateDTO planTemplate)
        {
            using (var searchClient = CreateAzureSearchClient())
            {
                using (var indexClient = searchClient.Indexes.GetClient(PlanTemplateIndexName))
                {
                    var document = ConvertToSearchDocument(planTemplate);

                    var batch = IndexBatch.New(
                        new IndexAction[]
                        {
                            IndexAction.Delete(document)
                        }
                    );
                    await indexClient.Documents.IndexAsync(batch);
                }
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
                    new Field("planTemplateId", DataType.String) { IsKey = true },
                    new Field("name", DataType.String) { IsSearchable = true, IsFilterable = true, IsSortable = true },
                    new Field("description", DataType.String) { IsSearchable = true, IsFilterable = true }
                }
            };

            return definition;
        }

        private Document ConvertToSearchDocument(PublishPlanTemplateDTO dto)
        {
            var document = new Document()
            {
                { "planTemplateId", dto.PlanTemplateId.ToString() },
                { "name", dto.Name },
                { "description", dto.Description }
            };

            return document;
        }

        private PublishPlanTemplateDTO ConvertSearchDocumentToDto(Document doc)
        {
            var dto = new PublishPlanTemplateDTO()
            {
                PlanTemplateId = Int32.Parse((string)doc["planTemplateId"]),
                Name = (string)doc["name"],
                Description = (string)doc["description"]
            };

            return dto;
        }

        private async Task EnsurePlanTemplateIndexExists(SearchServiceClient searchClient)
        {
            var exists = await searchClient.Indexes.ExistsAsync(PlanTemplateIndexName);
            if (!exists)
            {
                await searchClient.Indexes.CreateAsync(CreateIndexDefinition());
            }
        }

        private async Task DeletePlanTemplateIndexIfExists(SearchServiceClient searchClient)
        {
            var exists = await searchClient.Indexes.ExistsAsync(PlanTemplateIndexName);
            if (exists)
            {
                await searchClient.Indexes.DeleteAsync(PlanTemplateIndexName);
            }
        }
    }
}