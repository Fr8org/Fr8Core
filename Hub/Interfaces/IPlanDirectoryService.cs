using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IPlanDirectoryService
    {
        /// <summary>
        /// Get token for user authentication from Plan Directory 
        /// </summary>
        /// <param name="UserId">User who will be authenticated in PD</param>
        /// <returns></returns>
        Task<string> GetToken(string UserId);

        /// <summary>
        /// Get url for logging out
        /// </summary>
        /// <returns>url string</returns>
        string LogOutUrl();

        Task<PublishPlanTemplateDTO> GetTemplate(Guid id, string userId); 
        Task Share(Guid planId, string userId);
        Task Unpublish(Guid planId, string userId);
        PlanFullDTO CrateTemplate(Guid planId, string userId);
        PlanEmptyDTO CreateFromTemplate(PlanFullDTO plan, string userId);
    }
}
