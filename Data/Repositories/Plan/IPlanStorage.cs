using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using FluentValidation.Internal;

namespace Data.Repositories.Plan
{
    public class PlanStorage
    {
        private readonly IPlanCache _cache;
        private readonly IPlanStorageProvider _storageProvider;

        public PlanStorage(IPlanCache cache, IPlanStorageProvider storageProvider)
        {
            _cache = cache;
            _storageProvider = storageProvider;
        }

        public RouteNodeDO LoadPlan( Guid planMemberId)
        {
            lock (_cache)
            {
                return _cache.Get(planMemberId, _storageProvider.LoadPlan);
            }
        }
        
        public IQueryable<PlanDO> GetPlanQuery()
        {
            return _storageProvider.GetPlanQuery();
        }

        public IQueryable<ActivityDO> GetActivityQuery()
        {
            return _storageProvider.GetActivityQuery();
        }

        public IQueryable<RouteNodeDO> GetNodesQuery()
        {
            return _storageProvider.GetNodesQuery();
        }

        public void UpdateElement(Guid id, Action<RouteNodeDO> updater)
        {
            _cache.UpdateElement(id, updater);
        }

        public void UpdateElements(Action<RouteNodeDO> updater)
        {
            _cache.UpdateElements(updater);
        }

        public void Update(RouteNodeDO node)
        {
            lock (_cache)
            {
                var reference = _cache.Get(node.Id, _storageProvider.LoadPlan);
                var currentSnapshot = new RouteSnapshot(node, false);
                var referenceSnapshot = new RouteSnapshot(reference, false);
                var changes = currentSnapshot.Compare(referenceSnapshot);

                if (changes.HasChanges)
                {
                    _storageProvider.Update(changes);
                    _cache.Update(node);
                }
            }
        }
    }
}
