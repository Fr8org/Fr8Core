using System;
using Data.Entities;
using Fr8.Infrastructure.Data.DataTransferObjects.PlanTemplates;

namespace Hub.Interfaces
{
    public interface IPlanTemplates
    {
        PlanTemplateDTO GetPlanTemplate(Guid planId, string fr8UserId);
        PlanDO LoadPlan(PlanTemplateDTO planTemplate, string fr8UserId);
    }
}
