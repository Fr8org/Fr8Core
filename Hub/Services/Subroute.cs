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
        private readonly IAction _action;

        public Subroute()
        {
            _routeNode = ObjectFactory.GetInstance<IRouteNode>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _action = ObjectFactory.GetInstance<IAction>();
        }

        /// <summary>
        /// Create Subroute entity with required children criteria entity.
        /// </summary>
        public void Store(IUnitOfWork uow, SubrouteDO subroute )
        {
            if (subroute == null)
            {
                subroute = ObjectFactory.GetInstance<SubrouteDO>();
            }

            uow.SubrouteRepository.Add(subroute);
            
            // Saving criteria entity in repository.
            var criteria = new CriteriaDO()
            {
                Subroute = subroute,
                CriteriaExecutionType = CriteriaExecutionType.WithoutConditions
            };
            uow.CriteriaRepository.Add(criteria);
            
            //we don't want to save changes here, to enable upstream transactions
        }

        // <summary>
        /// Creates noew Subroute entity and add it to RouteDO. If RouteDO has no child subroute created plan becomes starting subroute.
        /// </summary>
        public SubrouteDO Create(IUnitOfWork uow, PlanDO plan, string name)
        {
            var subroute = new SubrouteDO();
            subroute.Id = Guid.NewGuid();
            subroute.RootRouteNode = plan;
            subroute.Fr8Account = plan.Fr8Account;

            uow.SubrouteRepository.Add(subroute);

            if (plan != null)
            {
                if (!plan.Subroutes.Any())
                {
                    plan.StartingSubroute = subroute;
                    subroute.StartingSubroute = true;
                }
            }

            subroute.Name = name;



            return subroute;
        }

        /// <summary>
        /// Update Subroute entity.
        /// </summary>
        public void Update(IUnitOfWork uow, SubrouteDO subroute)
        {
            if (subroute == null)
            {
                throw new Exception("Updating logic was passed a null SubrouteDO");
            }

            var curSubroute = uow.SubrouteRepository.GetByKey(subroute.Id);
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
            var subroute = uow.SubrouteRepository.GetByKey(id);

            if (subroute == null)
            {
                throw new Exception(string.Format("Unable to find Subroute by id = {0}", id));
            }

            // Remove all actions.

            

          //  subroute.Activities.ForEach(x => uow.ActivityRepository.Remove(x));

//            uow.SaveChanges();
//            
//            // Remove Criteria.
//            uow.CriteriaRepository
//                .GetQuery()
//                .Where(x => x.SubrouteId == id)
//                .ToList()
//                .ForEach(x => uow.CriteriaRepository.Remove(x));
//
//            uow.SaveChanges();

            // Remove Subroute.
            //uow.SubrouteRepository.Remove(subroute);


            ObjectFactory.GetInstance<IRouteNode>().Delete(uow, subroute);

            uow.SaveChanges();
        }

        public void AddAction(IUnitOfWork uow, ActionDO curActionDO)
        {
            var subroute = uow.SubrouteRepository.GetByKey(curActionDO.ParentRouteNodeId);

            if (subroute == null)
            {
                throw new Exception(string.Format("Unable to find Subroute by id = {0}", curActionDO.ParentRouteNodeId));
            }

            curActionDO.Ordering = subroute.ChildNodes.Count > 0 ? subroute.ChildNodes.Max(x => x.Ordering) + 1 : 1;

            subroute.ChildNodes.Add(curActionDO);

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
                var curAction = await uow.ActionRepository.GetQuery().SingleAsync(a => a.Id == actionId);
                var downstreamActions = _routeNode.GetDownstreamActivities(uow, curAction).OfType<ActionDO>();

                //set ActivityTemplate and parentRouteNode of current action to null -> to simulate a delete
                int? templateIdBackup = curAction.ActivityTemplateId;
                Guid? parentRouteNodeIdBackup = curAction.ParentRouteNodeId;
                curAction.ActivityTemplateId = null;
                curAction.ParentRouteNodeId = null;
                uow.SaveChanges();


                //lets start multithreaded calls
                var configureTaskList = new List<Task<ActionDTO>>();
                foreach (var downstreamAction in downstreamActions)
                {
                    configureTaskList.Add(_action.Configure(userId, downstreamAction, false));
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
                    curAction.ActivityTemplateId = templateIdBackup;
                    curAction.ParentRouteNodeId = parentRouteNodeIdBackup;
                    uow.SaveChanges();
                }
                else
                {
                    uow.ActionRepository.Remove(curAction);
                    uow.SaveChanges();
                    //TODO update ordering of downstream actions
                }

            }
            return validationErrors.Count < 1;
        }

        //TODO find a better response type for this function
        public async Task<bool> DeleteAction(string userId, Guid actionId, bool confirmed)
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

        protected void DeleteActionKludge(Guid id)
        {
            //Kludge solution
            //https://maginot.atlassian.net/wiki/display/SH/Action+Deletion+and+Reordering

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                var curAction = uow.RouteNodeRepository.GetQuery().FirstOrDefault(al => al.Id == id);
                if (curAction == null)
                {
                    throw new InvalidOperationException("Unknown RouteNode with id: " + id);
                }

                var downStreamActivities = _routeNode.GetDownstreamActivities(uow, curAction).OfType<ActionDO>();
                //we should clear values of configuration controls

                foreach (var downStreamActivity in downStreamActivities)
                {
                    var currentActivity = downStreamActivity;
                    bool somethingToReset = false;

                    using (var updater = _crate.UpdateStorage(() => currentActivity.CrateStorage))
                    {
                        foreach (var configurationControls in updater.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>())
                    {
                            foreach (IResettable resettable in configurationControls.Controls)
                        {
                                resettable.Reset();
                                somethingToReset = true;
                        }
                    }

                        if (!somethingToReset)
                    {
                            updater.DiscardChanges();
                        }
                    }
                }

                _routeNode.Delete(uow, curAction);
                uow.SaveChanges();
            }

        }
    }
}
