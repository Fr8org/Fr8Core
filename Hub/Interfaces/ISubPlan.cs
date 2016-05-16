using System;
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

        /// <summary>
        /// Extract first activity from subplan by SubPlan.Id.
        /// </summary>
       ActivityDO GetFirstActivity(IUnitOfWork uow, Guid subPlanId);
    }
}
