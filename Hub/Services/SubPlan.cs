using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using fr8.Infrastructure.Utilities.Logging;
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
                throw new Exception($"Unable to find plan by id = {subplan.RootPlanNodeId}");
            }

            var curPlanNode = uow.PlanRepository.GetById<PlanNodeDO>(subplan.ParentPlanNodeId);
            if (curPlanNode == null)
            {
                throw new Exception($"Unable to find parent plan-node by id = {subplan.ParentPlanNodeId}");
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
                throw new Exception(string.Format("Unable to find criteria by id = {0}", subplan.Id));
            }

            curSubPlan.Name = subplan.Name;
            curSubPlan.NodeTransitions = subplan.NodeTransitions;

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
                throw new Exception(string.Format("Unable to find Subplan by id = {0}", id));
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
                var message = "Subplan with given Id not found. Id=" + subPlanId;
                Logger.LogError(message);
                throw new ArgumentException(message);
            }
            return plan.ChildNodes.OfType<ActivityDO>().OrderBy(x => x.Ordering).FirstOrDefault();
        }
    }
}
