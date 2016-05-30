using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8Data.Manifests;
using PlanDirectory.Interfaces;
using Utilities.Configuration.Azure;

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

        public async Task CreateOrUpdate(string fr8AccountId, PublishPlanTemplateDTO planTemplate)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fr8Account = uow.UserRepository.GetByKey(fr8AccountId);
                if (fr8Account == null)
                {
                    throw new ApplicationException("Invalid Fr8AccountId.");
                }

                var planIdString = planTemplate.ParentPlanId.ToString();

                var existingPlanTemplateCM = uow.MultiTenantObjectRepository
                    .Query<PlanTemplateCM>(
                        fr8AccountId,
                        x => x.ParentPlanId == planIdString
                    )
                    .FirstOrDefault();

                var planTemplateCM = CreatePlanTemplateCM(
                    planTemplate,
                    existingPlanTemplateCM,
                    fr8Account
                );

                uow.MultiTenantObjectRepository.AddOrUpdate(
                    fr8AccountId,
                    planTemplateCM,
                    x => x.ParentPlanId == planIdString
                );

                uow.SaveChanges();
            }

            await Task.Yield();
        }

        public Task<PublishPlanTemplateDTO> Get(string fr8AccountId, Guid planId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var planIdString = planId.ToString();

                var planTemplateCM = uow.MultiTenantObjectRepository
                    .Query<PlanTemplateCM>(fr8AccountId, x => x.ParentPlanId == planIdString)
                    .FirstOrDefault();

                if (planTemplateCM == null)
                {
                    return Task.FromResult<PublishPlanTemplateDTO>(null);
                }

                return Task.FromResult(CreatePlanTemplateDTO(planTemplateCM));
            }
        }

        private PlanTemplateCM CreatePlanTemplateCM(PublishPlanTemplateDTO dto,
            PlanTemplateCM existing, Fr8AccountDO account)
        {
            return new PlanTemplateCM()
            {
                Name = dto.Name,
                Description = dto.Description,
                ParentPlanId = dto.ParentPlanId.ToString(),
                PlanContents = JsonConvert.SerializeObject(dto.PlanContents),
                Version = existing?.Version ?? 1,
                OwnerId = account.Id,
                OwnerName = account.UserName
            };
        }

        private PublishPlanTemplateDTO CreatePlanTemplateDTO(PlanTemplateCM planTemplate)
        {
            return new PublishPlanTemplateDTO()
            {
                Name = planTemplate.Name,
                Description = planTemplate.Description,
                ParentPlanId = Guid.Parse(planTemplate.ParentPlanId),
                PlanContents = JsonConvert.DeserializeObject<JToken>(planTemplate.PlanContents)
            };
        }

        // Commented out untill Azure Search Index activity is implemented.
        // public async Task<SearchResultDTO> Search(SearchRequestDTO request)
        // {
        //     using (var searchClient = CreateAzureSearchClient())
        //     {
        //         using (var indexClient = searchClient.Indexes.GetClient(PlanTemplateIndexName))
        //         {
        //             var sp = new SearchParameters();
        //             sp.OrderBy = new List<string>() { "name" };
        //             sp.IncludeTotalResultCount = true;
        // 
        //             if (request.PageStart > 0 && request.PageSize > 0)
        //             {
        //                 sp.Skip = (request.PageStart - 1) * request.PageSize;
        //                 sp.Top = request.PageSize;
        //             }
        // 
        //             var text = string.IsNullOrEmpty(request.Text) ? "*" : request.Text;
        // 
        //             var searchResult = await indexClient.Documents.SearchAsync(text, sp);
        //             var resultingDocuments = searchResult.Results.Select(x => ConvertSearchDocumentToDto(x.Document)).ToList();
        // 
        //             var dto = new SearchResultDTO()
        //             {
        //                 PlanTemplates = resultingDocuments,
        //                 TotalCount = searchResult.Count.GetValueOrDefault()
        //             };
        // 
        //             return dto;
        //         }
        //     }
        // }
        // 
        // public async Task Publish(PublishPlanTemplateDTO planTemplate)
        // {
        //     using (var searchClient = CreateAzureSearchClient())
        //     {
        //         using (var indexClient = searchClient.Indexes.GetClient(PlanTemplateIndexName))
        //         {
        // 
        //             var document = ConvertToSearchDocument(planTemplate);
        // 
        //             var batch = IndexBatch.New(
        //                 new IndexAction[]
        //                 {
        //                     IndexAction.Upload(document)
        //                 }
        //             );
        // 
        //             var indexResult = await indexClient.Documents.IndexAsync(batch);
        //         }
        //     }
        // }

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