using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Hub.Interfaces;
using StructureMap;

namespace Hub.Services.PlanDirectory
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

                var objectId = uow.MultiTenantObjectRepository
                    .GetObjectId<PlanTemplateCM>(fr8AccountId, x => x.ParentPlanId == planIdString);

                if (existingPlanTemplateCM == null && objectId.HasValue)
                {
                    ObjectFactory.GetInstance<ISecurityServices>().SetDefaultRecordBasedSecurityForObject(Roles.OwnerOfCurrentObject, objectId.Value, "Plan Template");
                }

                return Task.FromResult(planTemplateCM);
            }
        }

        public Task<PublishPlanTemplateDTO> GetPlanTemplateDTO(string fr8AccountId, Guid planId)
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

        public Task<PlanTemplateCM> Get(string fr8AccountId, Guid planId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var planIdString = planId.ToString();

                var planTemplateCM = uow.MultiTenantObjectRepository
                    .Query<PlanTemplateCM>(fr8AccountId, x => x.ParentPlanId == planIdString)
                    .FirstOrDefault();

                return Task.FromResult<PlanTemplateCM>(planTemplateCM);
            }
        }

        public async Task Remove(string fr8AccountId, Guid planId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var planIdStr = planId.ToString();
                uow.MultiTenantObjectRepository
                    .Delete<PlanTemplateCM>(
                        fr8AccountId,
                        x => x.ParentPlanId == planIdStr
                    );
                uow.SaveChanges();
                await Task.Yield();
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
                PlanContents = dto.PlanContents,
                Version = existing?.Version ?? 1,
                OwnerId = account.Id,
                OwnerName = account.UserName
            };
        }

        private PublishPlanTemplateDTO CreatePlanTemplateDTO(PlanTemplateCM planTemplate)
        {
            return new PublishPlanTemplateDTO
            {
                Name = planTemplate.Name,
                Description = planTemplate.Description,
                ParentPlanId = Guid.Parse(planTemplate.ParentPlanId),
                PlanContents = planTemplate.PlanContents
            };
        }
    }
}