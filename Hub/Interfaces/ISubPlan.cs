using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace Hub.Interfaces
{
    /// <summary>
    /// SubPlan service.
    /// </summary>
    public interface ISubPlan
    {
        void Update(IUnitOfWork uow, SubPlanDO subPlan);
        void Create(IUnitOfWork uow, SubPlanDO subPlan);
        Task Delete(IUnitOfWork uow, Guid id);
        Task<bool> DeleteAllChildNodes(Guid activityId);
        /// <summary>
        /// Backups current action and calls configure on downstream actions
        /// if there are validation errors restores current action and returns false
        /// </summary>
        /// <param name="userId">Current user id</param>
        /// <param name="activityId">Action to delete</param>
        /// <param name="confirmed">Forces deletion of current action even when there are validation errors</param>
        /// <returns>Deletion status of action</returns>
        Task<bool> DeleteActivity(string userId, Guid activityId, bool confirmed);

        /// <summary>
        /// Extract first activity from subplan by SubPlan.Id.
        /// </summary>
        Task<ActivityDO> GetFirstActivity(IUnitOfWork uow, Guid subPlanId);
    }
}
