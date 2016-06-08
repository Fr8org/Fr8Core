using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;

namespace PlanDirectory.Infrastructure
{
    public class PlanTemplate : IPlanTemplate
    {
        public Task<PlanTemplateCM> CreateOrUpdate(string fr8AccountId, PublishPlanTemplateDTO planTemplate)
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

                return Task.FromResult(planTemplateCM);
            }
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
    }
}