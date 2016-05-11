using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Interfaces;
using StructureMap;

namespace Hub.Services
{
    public class SubPlan : ISubPlan
    {

        private readonly IActivity _activity;
        private readonly ISecurityServices _security;

        public SubPlan()
        {
            _activity = ObjectFactory.GetInstance<IActivity>();
            _security = ObjectFactory.GetInstance<ISecurityServices>(); 
        }


        /// <summary>
        /// Create Subroute entity.
        /// </summary>
        public void Create(IUnitOfWork uow, SubPlanDO subPlan)
        {
            if (subPlan == null)
            {
                throw new ArgumentNullException(nameof(subPlan));
            }

            subPlan.Fr8Account = _security.GetCurrentAccount(uow);
            subPlan.CreateDate = DateTime.UtcNow;
            subPlan.LastUpdated = subPlan.CreateDate;

            var curPlan = uow.PlanRepository.GetById<PlanDO>(subPlan.RootPlanNodeId);
            if (curPlan == null)
            {
                throw new Exception($"Unable to find plan by id = {subPlan.RootPlanNodeId}");
            }

            var curPlanNode = uow.PlanRepository.GetById<PlanNodeDO>(subPlan.ParentPlanNodeId);
            if (curPlanNode == null)
            {
                throw new Exception($"Unable to find parent plan-node by id = {subPlan.ParentPlanNodeId}");
            }

            subPlan.RootPlanNode = curPlan;
            subPlan.ParentPlanNode = curPlanNode;

            curPlanNode.AddChildWithDefaultOrdering(subPlan);
        }

        /// <summary>
        /// Update Subroute entity.
        /// </summary>
        public void Update(IUnitOfWork uow, SubPlanDO subPlan)
        {
            if (subPlan == null)
            {
                throw new Exception("Updating logic was passed a null SubPlanDO");
            }

            var curSubPlan = uow.PlanRepository.GetById<SubPlanDO>(subPlan.Id);

            if (curSubPlan == null)
            {
                throw new Exception(string.Format("Unable to find criteria by id = {0}", subPlan.Id));
            }

            curSubPlan.Name = subPlan.Name;
            curSubPlan.NodeTransitions = subPlan.NodeTransitions;

            uow.SaveChanges();
        }

        /// <summary>
        /// Remove SubPlan and children entities by id.
        /// </summary>
        public async Task Delete(IUnitOfWork uow, Guid id)
        {
            var subPlan = uow.PlanRepository.GetById<SubPlanDO>(id);

            if (subPlan == null)
            {
                throw new Exception(string.Format("Unable to find SubPlan by id = {0}", id));
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
            var subPlan = uow.PlanRepository.GetById<SubPlanDO>(curActivityDO.ParentPlanNodeId.Value);

            if (subPlan == null)
            {
                throw new Exception(string.Format("Unable to find SubPlan by id = {0}", curActivityDO.ParentPlanNodeId));
            }

            subPlan.AddChildWithDefaultOrdering(curActivityDO);

            uow.SaveChanges();
        }

        public ActivityDO GetFirstActivity(IUnitOfWork uow, Guid subPlanId)
        {
            return uow.PlanRepository.GetById<PlanNodeDO>(subPlanId).ChildNodes.OfType<ActivityDO>().OrderBy(x => x.Ordering).FirstOrDefault();
        }
    }
}
