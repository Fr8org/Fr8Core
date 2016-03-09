using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories.Plan
{
    public class PlanStorageProviderEf : IPlanStorageProvider
    {
        protected readonly IUnitOfWork Uow;
        protected readonly RouteNodeRepository RouteNodes;
        protected readonly ActivityRepository ActivityRepository;
        protected readonly SubrouteRepository Subroutes;
        protected readonly RouteRepository Routes;

        public PlanStorageProviderEf(IUnitOfWork uow)
        {
            Uow = uow;
            RouteNodes = new RouteNodeRepository(uow);
            ActivityRepository = new ActivityRepository(uow);
            Subroutes = new SubrouteRepository(uow);
            Routes = new RouteRepository(uow);
        }

        public RouteNodeDO LoadPlan(Guid planMemberId)
        {
            var seed = RouteNodes.GetQuery().FirstOrDefault(x => x.Id == planMemberId);

            if (seed == null)
            {
                return null;
                throw new KeyNotFoundException("Unable to find route not with id = " + planMemberId);
            }

            var lookup = new Dictionary<Guid, RouteNodeDO>();

            var routes = Routes.GetQuery().Where(x => x.Id == seed.RootRouteNodeId).Include(x=>x.Fr8Account).AsEnumerable().Select(x => x.Clone()).ToArray();
            var actions = ActivityRepository.GetQuery().Where(x => x.RootRouteNodeId == seed.RootRouteNodeId).Include(x => x.Fr8Account).AsEnumerable().Select(x => x.Clone()).ToArray();
            var subroutes = Subroutes.GetQuery().Where(x => x.RootRouteNodeId == seed.RootRouteNodeId).Include(x => x.Fr8Account).AsEnumerable().Select(x => x.Clone()).ToArray();

            foreach (var routeNodeDo in routes)
            {
                lookup[routeNodeDo.Id] = routeNodeDo;
            }

            foreach (var routeNodeDo in actions)
            {
                lookup[routeNodeDo.Id] = routeNodeDo;
            }

            foreach (var routeNodeDo in subroutes)
            {
                lookup[routeNodeDo.Id] = routeNodeDo;
            }

            RouteNodeDO root = null;

            foreach (var routeNodeDo in lookup)
            {
                RouteNodeDO parent;

                if (routeNodeDo.Value.ParentRouteNodeId == null || !lookup.TryGetValue(routeNodeDo.Value.ParentRouteNodeId.Value, out parent))
                {
                    root = routeNodeDo.Value;
                    continue;
                }

                routeNodeDo.Value.ParentRouteNode = parent;
                parent.ChildNodes.Add(routeNodeDo.Value);
            }

            return root;
        }

        public virtual void Update(RouteSnapshot.Changes changes)
        {
            var adapter = (IObjectContextAdapter)Uow.Db;
            var objectContext = adapter.ObjectContext;

            ObjectStateEntry entry;

            foreach (var routeNodeDo in changes.Delete)
            {
                var entryStub = routeNodeDo.Clone();

                ClearNavigationProperties(entryStub);

                var key = objectContext.CreateEntityKey("RouteNodeDOes", entryStub);

                if (!objectContext.ObjectStateManager.TryGetObjectStateEntry(key, out entry))
                {
                    RouteNodes.Attach(entryStub);
                    entry = objectContext.ObjectStateManager.GetObjectStateEntry(entryStub);
                    entry.Delete();
                }
                else
                {
                    RouteNodes.Remove(routeNodeDo);
                }
            }

            foreach (var routeNodeDo in changes.Insert)
            {
                var entity = routeNodeDo.Clone();

                ClearNavigationProperties(entity);
                RouteNodes.Add(entity);
            }
            
            foreach (var changedObject in changes.Update)
            {
                var entryStub = changedObject.Node.Clone();

                ClearNavigationProperties(entryStub);

                var key = objectContext.CreateEntityKey("RouteNodeDOes", entryStub);
                if (!objectContext.ObjectStateManager.TryGetObjectStateEntry(key, out entry))
                {
                    RouteNodes.Attach(entryStub);
                    entry = objectContext.ObjectStateManager.GetObjectStateEntry(entryStub);
                    foreach (var changedProperty in changedObject.ChangedProperties)
                    {
                        entry.SetModifiedProperty(changedProperty.Name);
                    }
                }
                else
                {
                    foreach (var changedProperty in changedObject.ChangedProperties)
                    {
                        changedProperty.SetValue(entry.Entity, changedProperty.GetValue(changedObject.Node));
                    }
                }
            }
        }
        
        //we do this not to accidentatlly add duplicates
        protected void ClearNavigationProperties(RouteNodeDO entity)
        {
            entity.Fr8Account = null;
            entity.ParentRouteNode = null;

            var activity = entity as ActivityDO;
            if (activity != null)
            {
                activity.AuthorizationToken = null;
                activity.ActivityTemplate = null;
            }
        }

        public IQueryable<PlanDO> GetPlanQuery()
        {
            return Routes.GetQuery();
        }

        public IQueryable<ActivityDO> GetActivityQuery()
        {
            return ActivityRepository.GetQuery();
        }

        public IQueryable<RouteNodeDO> GetNodesQuery()
        {
            return RouteNodes.GetQuery();
        }
    }
}
