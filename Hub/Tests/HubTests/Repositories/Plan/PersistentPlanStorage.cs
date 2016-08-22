using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Repositories.Plan;

namespace HubTests.Repositories.Plan
{
    class PersistentPlanStorage : IPlanStorageProvider
    {
        private readonly Dictionary<Guid, PlanNodeDO> _storage = new Dictionary<Guid, PlanNodeDO>();
        
        public PersistentPlanStorage(PlanNodeDO root)
        {
            if (root == null)
            {
                return;
            }

            foreach (var node in PlanTreeHelper.Linearize(root))
            {
                _storage[node.Id] = node;
            }
        }

        public PlanNodeDO LoadPlan(Guid planMemberId)
        {
            PlanNodeDO root = null;

            foreach (var planNodeDo in _storage)
            {
                planNodeDo.Value.ParentPlanNode = null;
                planNodeDo.Value.ChildNodes.Clear();
            }

            foreach (var planNodeDo in _storage)
            {
                PlanNodeDO parent;

                if (planNodeDo.Value.ParentPlanNodeId == null || !_storage.TryGetValue(planNodeDo.Value.ParentPlanNodeId.Value, out parent))
                {
                    root = planNodeDo.Value;
                    continue;
                }

                planNodeDo.Value.ParentPlanNode = parent;
                parent.ChildNodes.Add(planNodeDo.Value);
            }

            return root;
        }
        
        public void Update(PlanSnapshot.Changes changes)
        {
            foreach (var change in changes.Delete)
            {
                _storage.Remove(change.Id);
            }

            foreach (var change in changes.Insert)
            {
                _storage.Add(change.Id, change);
            }

            foreach (var changedObject in changes.Update)
            {
                var obj = _storage[changedObject.Node.Id];
                foreach (var prop in changedObject.ChangedProperties)
                {
                    prop.SetValue(obj, prop.GetValue(changedObject.Node));
                }
            }
        }

        public IQueryable<PlanDO> GetPlanQuery()
        {
            throw new NotImplementedException();
        }

        public IQueryable<ActivityDO> GetActivityQuery()
        {
            throw new NotImplementedException();
        }

        public IQueryable<PlanNodeDO> GetNodesQuery()
        {
            throw new NotImplementedException();
        }
    }
}
