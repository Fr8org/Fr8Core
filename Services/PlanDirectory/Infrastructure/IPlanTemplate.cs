using System;
using System.Threading.Tasks;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Manifests;

namespace PlanDirectory.Infrastructure
{
    public interface IPlanTemplate
    {
        Task<PlanTemplateCM> CreateOrUpdate(string fr8AccountId, PublishPlanTemplateDTO planTemplate);
        Task<PublishPlanTemplateDTO> Get(string fr8AccountId, Guid planId);
    }
}
