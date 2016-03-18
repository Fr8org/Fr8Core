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

        public PlanNodeDO LoadPlan(Guid planMemberId)
        {
            var seed = PlanNodes.GetQuery().FirstOrDefault(x => x.Id == planMemberId);

            if (seed == null)
            {
                return null;
                throw new KeyNotFoundException("Unable to find plan not with id = " + planMemberId);
            }

            var lookup = new Dictionary<Guid, PlanNodeDO>();

            var plans = Plans.GetQuery().Where(x => x.Id == seed.RootPlanNodeId).Include(x=>x.Fr8Account).AsEnumerable().Select(x => x.Clone()).ToArray();
            var actions = ActivityRepository.GetQuery().Where(x => x.RootPlanNodeId == seed.RootPlanNodeId).Include(x => x.Fr8Account).AsEnumerable().Select(x => x.Clone()).ToArray();
            var subPlans = SubPlans.GetQuery().Where(x => x.RootPlanNodeId == seed.RootPlanNodeId).Include(x => x.Fr8Account).AsEnumerable().Select(x => x.Clone()).ToArray();

            foreach (var planNodeDo in plans)
            {
                lookup[planNodeDo.Id] = planNodeDo;
            }

            foreach (var planNodeDo in actions)
            {
                lookup[planNodeDo.Id] = planNodeDo;
            }

            foreach (var planNodeDo in subPlans)
            {
                lookup[planNodeDo.Id] = planNodeDo;
            }

            PlanNodeDO root = null;

            foreach (var planNodeDo in lookup)
            {
                PlanNodeDO parent;

                if (planNodeDo.Value.ParentPlanNodeId == null || !lookup.TryGetValue(planNodeDo.Value.ParentPlanNodeId.Value, out parent))
                {
                    root = planNodeDo.Value;
                    continue;
                }

                planNodeDo.Value.ParentPlanNode = parent;
                parent.ChildNodes.Add(planNodeDo.Value);
            }

            return root;
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
                    PlanNodes.Remove(planNodeDo);
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
