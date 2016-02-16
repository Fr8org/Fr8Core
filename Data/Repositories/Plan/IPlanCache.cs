using System;
using Data.Entities;

namespace Data.Repositories.Plan
{
    public interface IPlanCache
    {
        RouteNodeDO Get(Guid id, Func<Guid, RouteNodeDO> cacheMissCallback);
        void UpdateElement(Guid id, Action<RouteNodeDO> updater);
        void UpdateElements(Action<RouteNodeDO> updater);
        void Update(Guid id, RouteSnapshot.Changes changes);
    }
}