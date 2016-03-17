using System;
using System.Linq;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IPlanRepository
    {
        TPlanNode Reload<TPlanNode>(Guid id)
             where TPlanNode : PlanNodeDO;

        TPlanNode Reload<TPlanNode>(Guid? id)
            where TPlanNode : PlanNodeDO;

        TPlanNode GetById<TPlanNode>(Guid id)
            where TPlanNode : PlanNodeDO;

        TPlanNode GetById<TPlanNode>(Guid? id)
            where TPlanNode : PlanNodeDO;

        void Add(PlanDO plan);
        void Delete(PlanDO node);
        IQueryable<PlanDO> GetPlanQueryUncached();
        IQueryable<ActivityDO> GetActivityQueryUncached();
        IQueryable<PlanNodeDO> GetNodesQueryUncached();
        void RemoveAuthorizationTokenFromCache(ActivityDO activity);
    }
}