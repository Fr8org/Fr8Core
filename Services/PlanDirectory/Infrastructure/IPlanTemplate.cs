using System;
using System.Threading.Tasks;
using PlanDirectory.Interfaces;

namespace PlanDirectory.Infrastructure
{
    public interface IPlanTemplate
    {
        Task CreateOrUpdate(string fr8AccountId, PlanTemplateDTO planTemplate);
        Task<PlanTemplateDTO> Get(string fr8AccountId, Guid planId);
    }
}
