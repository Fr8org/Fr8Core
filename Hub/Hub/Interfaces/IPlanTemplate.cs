using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;

namespace Hub.Interfaces
{
    public interface IPlanTemplate
    {
        Task<PlanTemplateCM> CreateOrUpdate(string fr8AccountId, PublishPlanTemplateDTO planTemplate);
        Task<PublishPlanTemplateDTO> GetPlanTemplateDTO(string fr8AccountId, Guid planId);
        Task<PlanTemplateCM> Get(string fr8AccountId, Guid planId);
        Task Remove(string fr8AccountId, Guid planId);
    }
}
