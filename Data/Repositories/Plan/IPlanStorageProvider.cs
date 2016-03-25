using System;
using System.Linq;
using Data.Entities;

namespace Data.Repositories.Plan
{
    public interface IPlanStorageProvider
    {
        PlanNodeDO LoadPlan(Guid planMemberId);

        void Update(PlanSnapshot.Changes changes);

        IQueryable<PlanDO> GetPlanQuery();
        IQueryable<ActivityDO> GetActivityQuery();
        IQueryable<PlanNodeDO> GetNodesQuery();
    }
}
