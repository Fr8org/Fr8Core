using System;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace Hub.Interfaces
{
    /// <summary>
    /// Subplan service.
    /// </summary>
    public interface ISubplan
    {
        void Update(IUnitOfWork uow, SubplanDO subplan);
        void Create(IUnitOfWork uow, SubplanDO subplan);
        Task Delete(IUnitOfWork uow, Guid id);

        /// <summary>
        /// Extract first activity from subplan by Subplan.Id.
        /// </summary>
       ActivityDO GetFirstActivity(IUnitOfWork uow, Guid subPlanId);
    }
}
