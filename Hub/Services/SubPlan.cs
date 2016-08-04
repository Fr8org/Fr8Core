using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Fr8.Infrastructure;
using Fr8.Infrastructure.Utilities.Logging;
using Hub.Interfaces;
using StructureMap;

namespace Hub.Services
{
    public class Subplan : ISubplan
    {

        private readonly IActivity _activity;
        private readonly ISecurityServices _security;

        public Subplan()
        {
            _activity = ObjectFactory.GetInstance<IActivity>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
        }


        /// <summary>
        /// Create Subroute entity.
        /// </summary>
        public void Create(IUnitOfWork uow, SubplanDO subplan)
        {
            if (subplan == null)
            {
                throw new ArgumentNullException(nameof(subplan));
            }

            subplan.Fr8Account = _security.GetCurrentAccount(uow);
            subplan.CreateDate = DateTime.UtcNow;
            subplan.LastUpdated = subplan.CreateDate;

            var curPlan = uow.PlanRepository.GetById<PlanDO>(subplan.RootPlanNodeId);
            if (curPlan == null)
            {
                throw new MissingObjectException($"Plan with Id {subplan.RootPlanNodeId} doesn't exist");
            }

            var curPlanNode = uow.PlanRepository.GetById<PlanNodeDO>(subplan.ParentPlanNodeId);
            if (curPlanNode == null)
            {
                throw new MissingObjectException($"Parent plan-node with Id {subplan.ParentPlanNodeId} doesn't exist");
            }

            subplan.RootPlanNode = curPlan;
            subplan.ParentPlanNode = curPlanNode;

            curPlanNode.AddChildWithDefaultOrdering(subplan);
        }

        /// <summary>
        /// Update Subroute entity.
        /// </summary>
        public void Update(IUnitOfWork uow, SubplanDO subplan)
        {
            if (subplan == null)
            {
                throw new Exception("Updating logic was passed a null SubPlanDO");
            }

            var curSubPlan = uow.PlanRepository.GetById<SubplanDO>(subplan.Id);

            if (curSubPlan == null)
            {
                throw new MissingObjectException($"Subplan with Id {subplan.Id} doesn't exist");
            }

            curSubPlan.Name = subplan.Name;

            uow.SaveChanges();
        }

        /// <summary>
        /// Remove Subplan and children entities by id.
        /// </summary>
        public async Task Delete(IUnitOfWork uow, Guid id)
        {
            var subPlan = uow.PlanRepository.GetById<SubplanDO>(id);

            if (subPlan == null)
            {
                throw new MissingObjectException($"Subplan with Id {id} doesn't exist");
            }

            foreach (var activity in subPlan.ChildNodes.OfType<ActivityDO>())
            {
                await _activity.Delete(activity.Id);
            }

            subPlan.RemoveFromParent();

            uow.SaveChanges();
        }

        public void AddActivity(IUnitOfWork uow, ActivityDO curActivityDO)
        {
            var subPlan = uow.PlanRepository.GetById<SubplanDO>(curActivityDO.ParentPlanNodeId.Value);

            if (subPlan == null)
            {
                throw new Exception(string.Format("Unable to find Subplan by id = {0}", curActivityDO.ParentPlanNodeId));
            }

            subPlan.AddChildWithDefaultOrdering(curActivityDO);

            uow.SaveChanges();
        }

        public ActivityDO GetFirstActivity(IUnitOfWork uow, Guid subPlanId)
        {
            var plan = uow.PlanRepository.GetById<PlanNodeDO>(subPlanId);
            if (plan == null)
            {
                throw new MissingObjectException($"Subplan with Id {subPlanId} doesn't exist");
            }
            return plan.ChildNodes.OfType<ActivityDO>().OrderBy(x => x.Ordering).FirstOrDefault();
        }
    }
}
