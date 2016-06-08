using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;

namespace PlanDirectory.Infrastructure
{
    public interface IPlanTemplate
    {
        Task<PlanTemplateCM> CreateOrUpdate(string fr8AccountId, PublishPlanTemplateDTO planTemplate);
        Task<PublishPlanTemplateDTO> Get(string fr8AccountId, Guid planId);
        Task Remove(string fr8AccountId, Guid planId);
    }
}
