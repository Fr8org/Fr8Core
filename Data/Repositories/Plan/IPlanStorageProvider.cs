using System;
using System.Linq;
using Data.Entities;

namespace Data.Repositories.Plan
{
    public interface IPlanStorageProvider
    {
        RouteNodeDO LoadPlan(Guid planMemberId);
        void Update(RouteSnapshot.Changes changes);

        IQueryable<PlanDO> GetPlanQuery();
        IQueryable<ActivityDO> GetActivityQuery();
        IQueryable<RouteNodeDO> GetNodesQuery();
    }
}
