using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IPlanDirectoryService
    {
        Task<PublishPlanTemplateDTO> GetTemplate(Guid id, string userId); 
        Task Share(Guid planId, string userId);
        Task Unpublish(Guid planId, string userId, bool privileged);
        PlanDTO CrateTemplate(Guid planId, string userId);
        Task<PlanNoChildrenDTO> CreateFromTemplate(PlanDTO plan, string userId);
    }
}
