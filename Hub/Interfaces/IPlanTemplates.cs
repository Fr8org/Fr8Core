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
        PlanTemplateDTO SavePlan(Guid planId, string curFr8UserId);
        List<PlanTemplateDTO> GetTemplates(string userId);
        string LoadPlan(int planDescriptionId, string userId);
        PlanTemplateDTO GetTemplate(int planDescriptionId, string userId);
        void DeleteTemplate(int id, string userId);
    }
}
