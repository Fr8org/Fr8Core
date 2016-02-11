using System;
using System.Linq;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IPlanRepository
    {
        TRouteNode Reload<TRouteNode>(Guid id)
             where TRouteNode : RouteNodeDO;

        TRouteNode Reload<TRouteNode>(Guid? id)
            where TRouteNode : RouteNodeDO;

        TRouteNode GetById<TRouteNode>(Guid id)
            where TRouteNode : RouteNodeDO;

        TRouteNode GetById<TRouteNode>(Guid? id)
            where TRouteNode : RouteNodeDO;

        void Add(PlanDO plan);
        void Delete(PlanDO node);
        IQueryable<PlanDO> GetPlanQueryUncached();
        IQueryable<ActivityDO> GetActivityQueryUncached();
        IQueryable<RouteNodeDO> GetNodesQueryUncached();
        void RemoveAuthorizationTokenFromCache(ActivityDO activity);
    }
}