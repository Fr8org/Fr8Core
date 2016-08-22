using System;
using System.Linq;
using Data.Entities;

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

        public PlanNodeDO LoadPlan( Guid planMemberId)
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

        public IQueryable<PlanNodeDO> GetNodesQuery()
        {
            return _storageProvider.GetNodesQuery();
        }

        public void UpdateElement(Guid id, Action<PlanNodeDO> updater)
        {
            _cache.UpdateElement(id, updater);
        }

        public void UpdateElements(Action<PlanNodeDO> updater)
        {
            _cache.UpdateElements(updater);
        }

        public void Update(Guid planId, PlanSnapshot.Changes changes)
        {
            lock (_cache)
            {
                if (changes.HasChanges)
                {
                    var applicableChanges = _cache.Update(planId, changes);

                    if (applicableChanges.HasChanges)
                    {
                        _storageProvider.Update(applicableChanges);
                    }

                }
            }
        }
    }
}
