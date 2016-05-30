using System;
using System.Threading.Tasks;
using PlanDirectory.Interfaces;

namespace PlanDirectory.Infrastructure
{
    public interface IPlanTemplate
    {
        Task Initialize();

        Task CreateOrUpdate(string fr8AccountId, PublishPlanTemplateDTO planTemplate);
        Task<PublishPlanTemplateDTO> Get(string fr8AccountId, Guid planId);
    }
}
