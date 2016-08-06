using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects.PlanDirectory;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Interfaces;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace Hub.Services.PlanDirectory
{
    public class SearchProvider : ISearchProvider
    {
        public async Task Initialize(bool recreate)
        {
            using (var searchClient = CreateAzureSearchClient())
            {
                if (recreate)
                {
                    await DeletePlanTemplateIndexIfExists(searchClient);
                }

                await EnsurePlanTemplateIndexExists(searchClient);
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

        private string GetPlanTemplateIndexName()
        {
            var indexName = CloudConfigurationManager.GetSetting("AzureSearchIndexName");
            return indexName;
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
                Name = GetPlanTemplateIndexName(),
                Fields = new[]
                {
                    new Field("parentPlanId", DataType.String) { IsKey = true },
                    new Field("name", DataType.String) { IsSearchable = true, IsFilterable = true, IsSortable = true },
                    new Field("description", DataType.String) { IsSearchable = true, IsFilterable = true },
                    new Field("owner", DataType.String) { IsSearchable = true, IsFilterable = true }
                }
            };

            return definition;
        }

        private async Task EnsurePlanTemplateIndexExists(SearchServiceClient searchClient)
        {
            var exists = await searchClient.Indexes.ExistsAsync(GetPlanTemplateIndexName());
            if (!exists)
            {
                await searchClient.Indexes.CreateAsync(CreateIndexDefinition());
            }
        }

        private async Task DeletePlanTemplateIndexIfExists(SearchServiceClient searchClient)
        {
            var exists = await searchClient.Indexes.ExistsAsync(GetPlanTemplateIndexName());
            if (exists)
            {
                await searchClient.Indexes.DeleteAsync(GetPlanTemplateIndexName());
            }
        }

        public async Task<SearchResultDTO> Search(SearchRequestDTO request)
        {
            using (var searchClient = CreateAzureSearchClient())
            {
                using (var indexClient = searchClient.Indexes.GetClient(GetPlanTemplateIndexName()))
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
                    var resultingDocuments = searchResult.Results.Select(x => ConvertSearchDocumentToDTO(x.Document)).ToList();

                    var dto = new SearchResultDTO()
                    {
                        PlanTemplates = resultingDocuments,
                        TotalCount = searchResult.Count.GetValueOrDefault()
                    };

                    return dto;
                }
            }
        }

        public async Task CreateOrUpdate(PlanTemplateCM planTemplate)
        {
            using (var searchClient = CreateAzureSearchClient())
            {
                using (var indexClient = searchClient.Indexes.GetClient(GetPlanTemplateIndexName()))
                {
                    var document = ConvertToSearchDocument(planTemplate);
        
                    var batch = IndexBatch.New(
                        new IndexAction[]
                        {
                            IndexAction.MergeOrUpload(document)
                        }
                    );
        
                    await indexClient.Documents.IndexAsync(batch);
                }
            }
        }

        public async Task Remove(Guid planId)
        {
            using (var searchClient = CreateAzureSearchClient())
            {
                using (var indexClient = searchClient.Indexes.GetClient(GetPlanTemplateIndexName()))
                {
                    var doc = ConvertToSearchDocument(planId);

                    var batch = IndexBatch.New(new[] { IndexAction.Delete(doc) } );
                    await indexClient.Documents.IndexAsync(batch);
                }
            }
        }

        private Document ConvertToSearchDocument(Guid planId)
        {
            var document = new Document()
            {
                { "parentPlanId", planId.ToString() }
            };

            return document;
        }

        private Document ConvertToSearchDocument(PlanTemplateCM cm)
        {
            var document = new Document()
            {
                { "parentPlanId", cm.ParentPlanId.ToString() },
                { "name", cm.Name },
                { "description", cm.Description },
                { "owner", cm.OwnerName }
            };

            return document;
        }

        private SearchItemDTO ConvertSearchDocumentToDTO(Document document)
        {
            var dto = new SearchItemDTO()
            {
                ParentPlanId = Guid.Parse((string)document["parentPlanId"]),
                Name = (string)document["name"],
                Description = (string)document["description"],
                Owner = (string)document["owner"]
            };

            return dto;
        }
    }
}