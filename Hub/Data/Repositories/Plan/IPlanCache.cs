using System;
using Data.Entities;

namespace Data.Repositories.Plan
{
    public interface IPlanCache
    {
        PlanNodeDO Get(Guid id, Func<Guid, PlanNodeDO> cacheMissCallback);
        void UpdateElement(Guid id, Action<PlanNodeDO> updater);
        void UpdateElements(Action<PlanNodeDO> updater);
        PlanSnapshot.Changes Update(Guid id, PlanSnapshot.Changes changes);
    }
}