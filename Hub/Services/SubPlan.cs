using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using StructureMap;

namespace Hub.Services
{
    public class SubPlan : ISubPlan
    {

        private readonly ICrateManager _crate;
        private readonly IPlanNode _planNode;
        private readonly IActivity _activity;
        private readonly ISecurityServices _security;

        public SubPlan()
        {
            _planNode = ObjectFactory.GetInstance<IPlanNode>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _activity = ObjectFactory.GetInstance<IActivity>();
            _security = ObjectFactory.GetInstance<ISecurityServices>(); ;
        }

        /// <summary>
        /// Create SubPlan entity with required children criteria entity.
        /// </summary>
        public void Store(IUnitOfWork uow, SubPlanDO subPlan)
        {
            // IF we use this anymore?
            throw new NotImplementedException();
            /*if (subPlan == null)
            {
                subPlan = ObjectFactory.GetInstance<SubrouteDO>();
            }

            uow.PlanRepository.Add(subroute);
            
            // Saving criteria entity in repository.
            var criteria = new CriteriaDO()
            {
                SubPlan = subplan,
                CriteriaExecutionType = CriteriaExecutionType.WithoutConditions
            };
            uow.CriteriaRepository.Add(criteria);
            */
            //we don't want to save changes here, to enable upstream transactions
        }

        //        // <summary>
        //        /// Creates noew Subplan entity and add it to PlanDO. If PlanDO has no child subPlan created plan becomes starting subroute.
        //        /// </summary>
        //        public SubrouteDO Create(IUnitOfWork uow, PlanDO plan, string name)
        //        {
        //            var subPlan = new SubPlanDO();
        //            subPlan.Id = Guid.NewGuid();
        //            subPlan.RootPlanNode = plan;
        //            subPlan.Fr8Account = plan.Fr8Account;
        //
        //            uow.SubPlanRepository.Add(subPlan);
        //
        //            if (plan != null)
        //            {
        //                //if (!plan.SubPlans.Any())
        //                //{
        //                    plan.StartingSubPlan = subPlam;
        //                    subPlan.StartingSubPlan = true;
        //                //}
        //            }
        //
        //            subPlan.Name = name;
        //
        //
        //
        //            return subPlan;
        //        }

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

            await DeleteAllChildNodes(id);
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

        protected Crate<FieldDescriptionsCM> GetValidationErrors(CrateStorageDTO crateStorage)
        {
            return _crate.FromDto(crateStorage).FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == "Validation Errors");
        }

        /// <summary>
        /// This function gets called on first time user tries to delete an action
        /// which tries to validate all downstream actions and carries on deletion
        /// if there are validation errors on downstream crates it cancels deletion and returns false
        /// </summary>
        /// <param name="userId">current user id</param>
        /// <param name="actionId">action to delete</param>
        /// <returns>isActionDeleted</returns>
        protected async Task<bool> ValidateDownstreamActivitiesAndDelete(string userId, Guid actionId)
        {
            var validationErrors = new List<Crate>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAction = uow.PlanRepository.GetById<ActivityDO>(actionId);
                var downstreamActions = _planNode.GetDownstreamActivities(uow, curAction).OfType<ActivityDO>();
                var directChildren = curAction.GetDescendants().OfType<ActivityDO>();
                //set ActivityTemplate and parentPlanNode of current action to null -> to simulate a delete
                //int? templateIdBackup = curAction.ActivityTemplateId;
                PlanNodeDO parentPlanNodeIdBackup = curAction.ParentPlanNode;
                //curAction.ActivityTemplateId = null;
                curAction.RemoveFromParent();
                uow.SaveChanges();


                //lets start multithreaded calls
                var configureTaskList = new List<Task<ActivityDTO>>();
                // no sense of checking children of the action being deleted. We'll delete them in any case.
                foreach (var downstreamAction in downstreamActions.Except(directChildren))
                {
                    configureTaskList.Add(_activity.Configure(uow, userId, downstreamAction, false));
                }

                await Task.WhenAll(configureTaskList);

                //collect terminal responses
                //all tasks are completed by now
                var terminalResponseList = configureTaskList.Select(t => t.Result);

                foreach (var terminalResponse in terminalResponseList)
                {
                    var terminalValidationError = GetValidationErrors(terminalResponse.CrateStorage);
                    if (terminalValidationError != null)
                    {
                        validationErrors.Add(terminalValidationError);
                    }
                }

                //if there are validation errors restore curActionBackup
                if (validationErrors.Count > 0)
                {
                    //restore it
                    // curAction.ActivityTemplateId = templateIdBackup;
                    if (parentPlanNodeIdBackup != null)
                    {
                        parentPlanNodeIdBackup.ChildNodes.Add(curAction);
                    }

                    uow.SaveChanges();
                }
                else
                {
                    curAction.RemoveFromParent();
                    uow.SaveChanges();
                    //TODO update ordering of downstream actions
                }

            }
            return validationErrors.Count < 1;
        }

        public Task<ActivityDO> GetFirstActivity(IUnitOfWork uow, Guid subPlanId)
        {
            var result = uow.PlanRepository.GetActivityQueryUncached()
                .Where(x => x.ParentPlanNodeId == subPlanId)
                .OrderBy(x => x.Ordering)
                .FirstOrDefault();

            return Task.FromResult(result);
        }

        //TODO find a better response type for this function
        public async Task<bool> DeleteActivity(string userId, Guid actionId, bool confirmed)
        {
            if (confirmed)
            {
                //we can assume that there has been some validation errors on previous call
                //but user still wants to delete this action
                //lets use kludge solution
                DeleteActivityKludge(actionId);
            }
            else
            {
                return await ValidateDownstreamActivitiesAndDelete(userId, actionId);
            }
            return true;
        }

        public async Task<bool> DeleteAllChildNodes(Guid activityId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAction = uow.PlanRepository.GetById<PlanNodeDO>(activityId);
                PlanNodeDO currentAction = curAction;
                var downStreamActivities = _planNode.GetDownstreamActivities(uow, curAction).OfType<ActivityDO>();

                bool hasChanges = false;

                do
                {
                    currentAction = _planNode.GetNextActivity(currentAction, curAction);
                    if (currentAction != null)
                    {
                        hasChanges = true;
                        currentAction.RemoveFromParent();
                    }

                } while (currentAction != null);


                if (hasChanges)
                {
                    uow.SaveChanges();
                }
                //we should clear values of configuration controls

                foreach (var downStreamActivity in downStreamActivities)
                {
                    var currentActivity = downStreamActivity;
                    bool somethingToReset = false;

                    using (var crateStorage = _crate.UpdateStorage(() => currentActivity.CrateStorage))
                    {
                        foreach (var configurationControls in crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>())
                        {
                            foreach (IResettable resettable in configurationControls.Controls)
                            {
                                resettable.Reset();
                                somethingToReset = true;
                            }
                        }

                        if (!somethingToReset)
                        {
                            crateStorage.DiscardChanges();
                        }
                    }

                    // Detach containers from action, where CurrentPlanNodeId == id.
                    var containersWithCurrentPlanNode = uow.ContainerRepository
                        .GetQuery()
                        .Where(x => x.CurrentActivityId == downStreamActivity.Id)
                        .ToList();

                    containersWithCurrentPlanNode.ForEach(x => x.CurrentActivityId = null);

                    // Detach containers from action, where NextRouteNodeId == id.
                    var containersWithNextPlanNode = uow.ContainerRepository
                        .GetQuery()
                        .Where(x => x.NextActivityId == downStreamActivity.Id)
                        .ToList();

                    containersWithNextPlanNode.ForEach(x => x.NextActivityId = null);
                }

                uow.SaveChanges();
            }

            return await Task.FromResult(true);
        }

        protected void DeleteActivityKludge(Guid id)
        {
            //Kludge solution
            //https://maginot.atlassian.net/wiki/display/SH/Action+Deletion+and+Reordering

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAction = uow.PlanRepository.GetById<PlanNodeDO>(id);
                if (curAction == null)
                {
                    throw new InvalidOperationException("Unknown PlanNode with id: " + id);
                }

                // Detach containers from action, where CurrentPlanNodeId == id.
                var containersWithCurrentPlanNode = uow.ContainerRepository
                    .GetQuery()
                    .Where(x => x.CurrentActivityId == id)
                    .ToList();

                containersWithCurrentPlanNode.ForEach(x => x.CurrentActivityId = null);

                // Detach containers from action, where NextRouteNodeId == id.
                var containersWithNextPlanNode = uow.ContainerRepository
                    .GetQuery()
                    .Where(x => x.NextActivityId == id)
                    .ToList();

                containersWithNextPlanNode.ForEach(x => x.NextActivityId = null);

                uow.SaveChanges();


                var downStreamActivities = _planNode.GetDownstreamActivities(uow, curAction).OfType<ActivityDO>();
                //we should clear values of configuration controls

                foreach (var downStreamActivity in downStreamActivities)
                {
                    var currentActivity = downStreamActivity;
                    bool somethingToReset = false;

                    using (var crateStorage = _crate.UpdateStorage(() => currentActivity.CrateStorage))
                    {
                        foreach (var configurationControls in crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>())
                        {
                            foreach (IResettable resettable in configurationControls.Controls)
                            {
                                resettable.Reset();
                                somethingToReset = true;
                            }
                        }

                        if (!somethingToReset)
                        {
                            crateStorage.DiscardChanges();
                        }
                    }
                }

                _planNode.Delete(uow, curAction);
                uow.SaveChanges();
            }

        }
    }
}
