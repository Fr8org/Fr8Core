using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories.Plan
{
    public class PlanStorageProviderEf : IPlanStorageProvider
    {
        protected readonly IUnitOfWork Uow;
        // Note repository Names are not renamed .....
        protected readonly PlanNodeRepository PlanNodes;
        protected readonly ActivityRepository ActivityRepository;
        protected readonly SubPlanRepository SubPlans;
        protected readonly PlansRepository Plans;

        public PlanStorageProviderEf(IUnitOfWork uow)
        {
            Uow = uow;
            PlanNodes = new PlanNodeRepository(uow);
            ActivityRepository = new ActivityRepository(uow);
            SubPlans = new SubPlanRepository(uow);
            Plans = new PlansRepository(uow);
        }

        private PlanNodeDO LoadPlanByPlanId(Guid planId)
        {
            var lookup = new Dictionary<Guid, PlanNodeDO>();

            var plans = Plans.GetQuery().Where(x => x.Id == planId).Include(x => x.Fr8Account).AsEnumerable().Select(x => x.Clone()).ToArray();

            if (plans.Length == 0)
            {
                return null;
                //throw new KeyNotFoundException("Unable to find plan not with id = " + planId);
            }

            var actions = ActivityRepository.GetQuery().Where(x => x.RootPlanNodeId == planId).Include(x => x.Fr8Account).AsEnumerable().Select(x => x.Clone()).ToArray();
            var subPlans = SubPlans.GetQuery().Where(x => x.RootPlanNodeId == planId).Include(x => x.Fr8Account).AsEnumerable().Select(x => x.Clone()).ToArray();

            if (actions.Length == 0 && subPlans.Length == 0)
            {
                var nodes = PlanNodes.GetQuery().Where(x => x.RootPlanNodeId == planId).Include(x => x.Fr8Account).AsEnumerable().Select(x => x.Clone()).ToArray();

                if (nodes.Length == 0)
                {
                    throw new NotSupportedException($"Completely empty plans like {planId} are not supported");
                }

                foreach (var planNodeDo in nodes)
                {
                    lookup[planNodeDo.Id] = planNodeDo;
                }
            }
            else
            {
                foreach (var planNodeDo in actions)
                {
                    lookup[planNodeDo.Id] = planNodeDo;
                }

                foreach (var planNodeDo in subPlans)
                {
                    lookup[planNodeDo.Id] = planNodeDo;
                }
            }

            foreach (var planNodeDo in plans)
            {
                lookup[planNodeDo.Id] = planNodeDo;
            }
            
            PlanNodeDO root = null;
            var pendingNodes = new Stack<KeyValuePair<Guid, PlanNodeDO>>();

            foreach (var planNodeDo in lookup)
            {
                pendingNodes.Push(planNodeDo);
            }

            while (pendingNodes.Count > 0)
            {
                var planNodeDo = pendingNodes.Pop();

                PlanNodeDO parent;

                if (planNodeDo.Value.ParentPlanNodeId == null)
                {
                    root = planNodeDo.Value;
                    continue;
                }

                //We are... 
                //We are...
                //We are loading the broken plan
                if (!lookup.TryGetValue(planNodeDo.Value.ParentPlanNodeId.Value, out parent))
                {
                    var node = PlanNodes.GetQuery().Include(x => x.Fr8Account).FirstOrDefault(x => x.Id == planNodeDo.Value.ParentPlanNodeId.Value);

                    //This plan... 
                    //This plan...
                    //Was broken from the start
                    if (node == null)
                    {
                        throw new Exception($"Plan {planId} is completely broken. It has node {planNodeDo.Key} that references non existing parent {planNodeDo.Value.ParentPlanNodeId.Value}");
                    }

                    node = node.Clone();

                    lookup[node.Id] = node;
                    parent = node;
                    pendingNodes.Push(new KeyValuePair<Guid, PlanNodeDO>(node.Id, node));
                }

                planNodeDo.Value.ParentPlanNode = parent;
                parent.ChildNodes.Add(planNodeDo.Value);
            }

            return root;
        }

        public PlanNodeDO LoadPlan(Guid planMemberId)
        {
            var seed = PlanNodes.GetQuery().FirstOrDefault(x => x.Id == planMemberId);

            if (seed == null)
            {
                return null;
                //throw new KeyNotFoundException("Unable to find plan that has memeber with id = " + planMemberId);
            }

            if (seed.RootPlanNodeId == null)
            {
                throw new InvalidOperationException($"PlanNodes table is unconsistent. Node {planMemberId} doesn't have RootPlanNodeId set.");
            }

            return LoadPlanByPlanId(seed.RootPlanNodeId.Value);
        }

        public virtual void Update(PlanSnapshot.Changes changes)
        {
            var adapter = (IObjectContextAdapter)Uow.Db;
            var objectContext = adapter.ObjectContext;

            ObjectStateEntry entry;

            foreach (var planNodeDo in changes.Delete)
            {
                var entryStub = planNodeDo.Clone();

                ClearNavigationProperties(entryStub);

                var key = objectContext.CreateEntityKey("PlanNodeDOes", entryStub);

                if (!objectContext.ObjectStateManager.TryGetObjectStateEntry(key, out entry))
                {
                    PlanNodes.Attach(entryStub);
                    entry = objectContext.ObjectStateManager.GetObjectStateEntry(entryStub);
                    entry.Delete();
                }
                else
                {
                    var planNodeFromObjectContext = objectContext.GetObjectByKey(key);
                    PlanNodes.Remove((PlanNodeDO)planNodeFromObjectContext);
                }
            }

            foreach (var planNodeDo in changes.Insert)
            {
                var entity = planNodeDo.Clone();

                ClearNavigationProperties(entity);
                PlanNodes.Add(entity);
            }
            
            foreach (var changedObject in changes.Update)
            {
                var entryStub = changedObject.Node.Clone();

                ClearNavigationProperties(entryStub);

                var key = objectContext.CreateEntityKey("PlanNodeDOes", entryStub);
                if (!objectContext.ObjectStateManager.TryGetObjectStateEntry(key, out entry))
                {
                    PlanNodes.Attach(entryStub);
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
        protected void ClearNavigationProperties(PlanNodeDO entity)
        {
            entity.Fr8Account = null;
            entity.ParentPlanNode = null;

            var activity = entity as ActivityDO;
            if (activity != null)
            {
                activity.AuthorizationToken = null;
                activity.ActivityTemplate = null;
            }
        }

        public IQueryable<PlanDO> GetPlanQuery()
        {
            return Plans.GetQuery();
        }

        public IQueryable<ActivityDO> GetActivityQuery()
        {
            return ActivityRepository.GetQuery();
        }

        public IQueryable<PlanNodeDO> GetNodesQuery()
        {
            return PlanNodes.GetQuery();
        }
    }
}
