using Data.Entities;
using Data.Interfaces.DataTransferObjects.PlanTemplates;
using Fr8Data.DataTransferObjects.PlanTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Interfaces
{
    public interface IPlanTemplates
    {
        PlanTemplateDTO GetPlanTemplate(Guid planId, string fr8UserId);
        PlanDO LoadPlan(PlanTemplateDTO planTemplate, string fr8UserId);

        // Commented out by yakov.gnusin,
        // since we're using new approach described in https://maginot.atlassian.net/wiki/display/SH/Plan+Directory+Architecture

        // PlanTemplateDTO SavePlan(Guid planId, string curFr8UserId);
        // List<PlanTemplateDTO> GetTemplates(string userId);
        // string LoadPlan(int planDescriptionId, string userId);
        // PlanTemplateDTO GetTemplate(int planDescriptionId, string userId);
        // void DeleteTemplate(int id, string userId);
    }
}
