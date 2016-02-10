using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Repositories.Plan;

namespace DockyardTest.Repositories.Plan
{
    class PersistentPlanStorage : IPlanStorageProvider
    {
        private readonly Dictionary<Guid, RouteNodeDO> _storage = new Dictionary<Guid, RouteNodeDO>();
        
        public PersistentPlanStorage(RouteNodeDO root)
        {
            if (root == null)
            {
                return;
            }

            foreach (var node in RouteTreeHelper.Linearize(root))
            {
                _storage[node.Id] = node;
            }
        }

        public RouteNodeDO LoadPlan(Guid planMemberId)
        {
            RouteNodeDO root = null;

            foreach (var routeNodeDo in _storage)
            {
                routeNodeDo.Value.ParentRouteNode = null;
                routeNodeDo.Value.ChildNodes.Clear();
            }

            foreach (var routeNodeDo in _storage)
            {
                RouteNodeDO parent;

                if (routeNodeDo.Value.ParentRouteNodeId == null || !_storage.TryGetValue(routeNodeDo.Value.ParentRouteNodeId.Value, out parent))
                {
                    root = routeNodeDo.Value;
                    continue;
                }

                routeNodeDo.Value.ParentRouteNode = parent;
                parent.ChildNodes.Add(routeNodeDo.Value);
            }

            return root;
        }

        public void Update(RouteSnapshot.Changes changes)
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

        public IQueryable<RouteNodeDO> GetNodesQuery()
        {
            throw new NotImplementedException();
        }
    }
}
