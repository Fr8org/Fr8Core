using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using StructureMap;

namespace Hub.Services
{
    public class Subroute : ISubroute
    {

        private readonly ICrateManager _crate;
        private readonly IRouteNode _routeNode;
        private readonly IActivity _activity;

        public Subroute()
        {
            _routeNode = ObjectFactory.GetInstance<IRouteNode>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _activity = ObjectFactory.GetInstance<IActivity>();
        }

        /// <summary>
        /// Create Subroute entity with required children criteria entity.
        /// </summary>
        public void Store(IUnitOfWork uow, SubrouteDO subroute )
        {
            // IF we use this anymore?
            throw new NotImplementedException();
            /*if (subroute == null)
            {
                subroute = ObjectFactory.GetInstance<SubrouteDO>();
            }

            uow.PlanRepository.Add(subroute);
            
            // Saving criteria entity in repository.
            var criteria = new CriteriaDO()
            {
                Subroute = subroute,
                CriteriaExecutionType = CriteriaExecutionType.WithoutConditions
            };
            uow.CriteriaRepository.Add(criteria);
            */
            //we don't want to save changes here, to enable upstream transactions
        }

//        // <summary>
//        /// Creates noew Subroute entity and add it to RouteDO. If RouteDO has no child subroute created plan becomes starting subroute.
//        /// </summary>
//        public SubrouteDO Create(IUnitOfWork uow, PlanDO plan, string name)
//        {
//            var subroute = new SubrouteDO();
//            subroute.Id = Guid.NewGuid();
//            subroute.RootRouteNode = plan;
//            subroute.Fr8Account = plan.Fr8Account;
//
//            uow.SubrouteRepository.Add(subroute);
//
//            if (plan != null)
//            {
//                //if (!plan.Subroutes.Any())
//                //{
//                    plan.StartingSubroute = subroute;
//                    subroute.StartingSubroute = true;
//                //}
//            }
//
//            subroute.Name = name;
//
//
//
//            return subroute;
//        }

        /// <summary>
        /// Update Subroute entity.
        /// </summary>
        public void Update(IUnitOfWork uow, SubrouteDO subroute)
        {
            if (subroute == null)
            {
                throw new Exception("Updating logic was passed a null SubrouteDO");
            }

            var curSubroute =  uow.PlanRepository.GetById<SubrouteDO>(subroute.Id);
            
            if (curSubroute == null)
            {
                throw new Exception(string.Format("Unable to find criteria by id = {0}", subroute.Id));
            }

            curSubroute.Name = subroute.Name;
            curSubroute.NodeTransitions = subroute.NodeTransitions;
            
            uow.SaveChanges();
        }

        /// <summary>
        /// Remove Subroute and children entities by id.
        /// </summary>
        public void Delete(IUnitOfWork uow, Guid id)
        {
            var subroute = uow.PlanRepository.GetById<SubrouteDO>(id);

            if (subroute == null)
            {
                throw new Exception(string.Format("Unable to find Subroute by id = {0}", id));
            }

            subroute.RemoveFromParent();

            uow.SaveChanges();
        }

        public void AddActivity(IUnitOfWork uow, ActivityDO curActivityDO)
        {
            var subroute = uow.PlanRepository.GetById<SubrouteDO>(curActivityDO.ParentRouteNodeId.Value);

            if (subroute == null)
            {
                throw new Exception(string.Format("Unable to find Subroute by id = {0}", curActivityDO.ParentRouteNodeId));
            }

            subroute.AddChildWithDefaultOrdering(curActivityDO);

            uow.SaveChanges();
        }

        protected Crate<StandardDesignTimeFieldsCM> GetValidationErrors(CrateStorageDTO crateStorage)
        {
            return _crate.FromDto(crateStorage).FirstCrateOrDefault<StandardDesignTimeFieldsCM>(x => x.Label == "Validation Errors");
        }

        /// <summary>
        /// This function gets called on first time user tries to delete an action
        /// which tries to validate all downstream actions and carries on deletion
        /// if there are validation errors on downstream crates it cancels deletion and returns false
        /// </summary>
        /// <param name="userId">current user id</param>
        /// <param name="actionId">action to delete</param>
        /// <returns>isActionDeleted</returns>
        protected async Task<bool> ValidateDownstreamActionsAndDelete(string userId, Guid actionId)
        {
            var validationErrors = new List<Crate>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAction = uow.PlanRepository.GetById<ActivityDO>(actionId);
                var downstreamActions = _routeNode.GetDownstreamActivities(uow, curAction).OfType<ActivityDO>();
                var directChildren = curAction.GetDescendants().OfType<ActivityDO>();
                //set ActivityTemplate and parentRouteNode of current action to null -> to simulate a delete
                //int? templateIdBackup = curAction.ActivityTemplateId;
                RouteNodeDO parentRouteNodeIdBackup = curAction.ParentRouteNode;
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
                    if (parentRouteNodeIdBackup != null)
                    {
                        parentRouteNodeIdBackup.ChildNodes.Add(curAction);
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

        //TODO find a better response type for this function
        public async Task<bool> DeleteActivity(string userId, Guid actionId, bool confirmed)
        {
            if (confirmed)
            {
                //we can assume that there has been some validation errors on previous call
                //but user still wants to delete this action
                //lets use kludge solution
                DeleteActionKludge(actionId);
            }
            else
            {
                return await ValidateDownstreamActionsAndDelete(userId, actionId);
            }
            return true;
        }

        public async Task<bool> DeleteAllChildNodes(Guid activityId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAction = uow.PlanRepository.GetById<RouteNodeDO>(activityId);
                RouteNodeDO currentAction = curAction;
                var downStreamActivities = _routeNode.GetDownstreamActivities(uow, curAction).OfType<ActivityDO>();

                   bool hasChanges = false;
                 
                do
                {
                    currentAction = _routeNode.GetNextActivity(currentAction, curAction);
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
                //todo: check clear of container for main activity

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

                uow.SaveChanges();
            }

            return await Task.FromResult(true);
        }

        protected void DeleteActionKludge(Guid id)
        {
            //Kludge solution
            //https://maginot.atlassian.net/wiki/display/SH/Action+Deletion+and+Reordering

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAction = uow.PlanRepository.GetById<RouteNodeDO>(id);
                if (curAction == null)
                {
                    throw new InvalidOperationException("Unknown RouteNode with id: " + id);
                }

                // Detach containers from action, where CurrentRouteNodeId == id.
                var containersWithCurrentRouteNode = uow.ContainerRepository
                    .GetQuery()
                    .Where(x => x.CurrentRouteNodeId == id)
                    .ToList();

                containersWithCurrentRouteNode.ForEach(x => x.CurrentRouteNodeId = null);

                // Detach containers from action, where NextRouteNodeId == id.
                var containersWithNextRouteNode = uow.ContainerRepository
                    .GetQuery()
                    .Where(x => x.NextRouteNodeId == id)
                    .ToList();

                containersWithNextRouteNode.ForEach(x => x.NextRouteNodeId = null);

                uow.SaveChanges();


                var downStreamActivities = _routeNode.GetDownstreamActivities(uow, curAction).OfType<ActivityDO>();
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

                _routeNode.Delete(uow, curAction);
                uow.SaveChanges();
            }

        }
    }
}
