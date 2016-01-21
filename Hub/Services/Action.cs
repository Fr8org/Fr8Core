using System.Globalization;
using AutoMapper;
using Data.Constants;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Data.Control;
using Data.Crates;

using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Hub.Managers.APIManagers.Transmitters.Terminal;

namespace Hub.Services
{
    public class Action : IAction
    {
        private readonly ICrateManager _crate;
        private readonly IAuthorization _authorizationToken;

        private readonly IRouteNode _routeNode;

        public Action()
        {
            _authorizationToken = ObjectFactory.GetInstance<IAuthorization>();
            _routeNode = ObjectFactory.GetInstance<IRouteNode>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        public IEnumerable<TViewModel> GetAllActions<TViewModel>()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ActionRepository.GetAll().Select(Mapper.Map<TViewModel>);
            }
        }

        public ActionDO SaveOrUpdateAction(IUnitOfWork uow, ActionDO submittedActionData)
        {
            var action = SaveAndUpdateRecursive(uow, submittedActionData, null, new List<ActionDO>());

            action.ParentRouteNode = submittedActionData.ParentRouteNode;
            action.ParentRouteNodeId = submittedActionData.ParentRouteNodeId;

            uow.SaveChanges();

            return uow.ActionRepository.GetByKey(submittedActionData.Id);
        }

        public ActionDO SaveOrUpdateAction(ActionDO submittedActionData)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return SaveOrUpdateAction(uow, submittedActionData);
            }
        }

        public ActionDO GetById(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return GetById(uow, id);
            }
        }

        /// <summary>
        /// Update properties and structure of the actions and all descendats.
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="submittedActionData"></param>
        /// <returns></returns>
        //        public async Task<ActionDO> SaveUpdateAndConfigure(IUnitOfWork uow, ActionDO submittedActionData)
        //        {
        //            var pendingConfigurations = new List<ActionDO>();
        //            // Update properties and structure recisurively
        //            var existingAction = SaveAndUpdateRecursive(uow, submittedActionData, pendingConfigurations);
        //
        //            // Change parent if it is necessary
        //            existingAction.ParentRouteNode = submittedActionData.ParentRouteNode;
        //            existingAction.ParentRouteNodeId = submittedActionData.ParentRouteNodeId;
        //
        //            uow.SaveChanges();
        //
        //            foreach (var pendingConfiguration in pendingConfigurations)
        //            {
        //                await ConfigureSingleAction(uow, pendingConfiguration);
        //            }
        //
        //            return uow.ActionRepository.GetByKey(existingAction.Id);
        //        }

        private void UpdateActionProperties(IUnitOfWork uow, ActionDO submittedAction)
        {
            var existingAction = uow.ActionRepository.GetByKey(submittedAction.Id);

            if (existingAction == null)
            {
                throw new Exception("Action was not found");
            }

            UpdateActionProperties(existingAction, submittedAction);
            uow.SaveChanges();
        }

        private static void UpdateActionProperties(ActionDO existingAction, ActionDO submittedAction)
        {
            existingAction.ActivityTemplateId = submittedAction.ActivityTemplateId;
            existingAction.Name = submittedAction.Name;
            existingAction.Label = submittedAction.Label;
            existingAction.CrateStorage = submittedAction.CrateStorage;
            existingAction.Ordering = submittedAction.Ordering;
            existingAction.Fr8Account = submittedAction.Fr8Account;
        }

        private ActionDO SaveAndUpdateRecursive(IUnitOfWork uow, ActionDO submittedAction, ActionDO parent, List<ActionDO> pendingConfiguration)
        {
            ActionDO existingAction;

            if (submittedAction.ActivityTemplateId == 0)
            {
                submittedAction.ActivityTemplateId = null;
            }

            if (submittedAction.IsTempId)
            {
                if (submittedAction.Id == Guid.Empty)
                {
                    submittedAction.Id = Guid.NewGuid();
                }

                existingAction = submittedAction;
                submittedAction.IsTempId = false;

                if (submittedAction.ActivityTemplateId != null)
                {
                    submittedAction.ActivityTemplate = uow.ActivityTemplateRepository.GetByKey(submittedAction.ActivityTemplateId.Value);
                }

                RouteNodeDO subroute = null;

                if (parent == null)
                {
                    if (submittedAction.ParentRouteNodeId != null)
                    {
                        subroute = uow.RouteNodeRepository.GetByKey(submittedAction.ParentRouteNodeId);
                    }
                }
                else
                {
                    subroute = parent;
                }

                if (subroute == null)
                {
                    throw new Exception(string.Format("Unable to find Subroute by id = {0}", submittedAction.ParentRouteNodeId));
                }

                submittedAction.Ordering = subroute.ChildNodes.Count > 0 ? subroute.ChildNodes.Max(x => x.Ordering) + 1 : 1;

                //assign Fr8Account from Route -> Action -> ...
                submittedAction.Fr8Account = (subroute.RootRouteNode != null) ? subroute.Fr8Account : null;

                // Add Action to repo.
                uow.ActionRepository.Add(submittedAction);

                // If we have created new action add it to pending configuration list.
                pendingConfiguration.Add(submittedAction);

                foreach (var newAction in submittedAction.ChildNodes.OfType<ActionDO>())
                {
                    newAction.ParentRouteNodeId = null;
                    newAction.ParentRouteNode = null;
                    newAction.RootRouteNodeId = submittedAction.RootRouteNodeId;



                    var newChild = SaveAndUpdateRecursive(uow, newAction, existingAction, pendingConfiguration);
                    existingAction.ChildNodes.Add(newChild);
                }
            }
            else
            {
                existingAction = uow.ActionRepository.GetByKey(submittedAction.Id);

                if (existingAction == null)
                {
                    throw new Exception("Action was not found");
                }

                // Update properties
                UpdateActionProperties(existingAction, submittedAction);

                // Sync nested action structure
                if (submittedAction.ChildNodes != null)
                {
                    // Dictionary is used to avoid O(action.ChildNodes.Count*existingAction.ChildNodes.Count) complexity when computing difference between sets. 
                    // desired set of children. 
                    var newChildren = submittedAction.ChildNodes.OfType<ActionDO>().Where(x => !x.IsTempId).ToDictionary(x => x.Id, y => y);
                    // current set of children
                    var currentChildren = existingAction.ChildNodes.OfType<ActionDO>().ToDictionary(x => x.Id, y => y);

                    // Now we must find what child must be added to existingAction
                    // Chilren to be added are difference between set newChildren and currentChildren (those elements that exist in newChildren but do not exist in currentChildren).
                    foreach (var newAction in submittedAction.ChildNodes.OfType<ActionDO>().Where(x => x.IsTempId || !currentChildren.ContainsKey(x.Id)).ToArray())
                    {
                        newAction.ParentRouteNodeId = null;
                        newAction.ParentRouteNode = null;
                        newAction.RootRouteNodeId = existingAction.RootRouteNodeId;

                        var newChild = SaveAndUpdateRecursive(uow, newAction, existingAction, pendingConfiguration);
                        existingAction.ChildNodes.Add(newChild);
                    }



                    // Now we must find what child must be removed from existingAction
                    // Chilren to be removed are difference between set currentChildren and newChildren (those elements that exist in currentChildren but do not exist in newChildren).
                    foreach (var actionToRemove in currentChildren.Where(x => !newChildren.ContainsKey(x.Key)).ToArray())
                    {
                        existingAction.ChildNodes.Remove(actionToRemove.Value);
                        //i (bahadir) commented out this line. currently our deletion mechanism already removes this action from it's parent
                        //TODO talk to Vladimir about this
                        //    _routeNode.Delete(uow, actionToRemove.Value);
                    }
                    // We just update those children that haven't changed (exists both in newChildren and currentChildren)
                    foreach (var actionToUpdate in newChildren.Where(x => !x.Value.IsTempId && currentChildren.ContainsKey(x.Key)))
                    {
                        SaveAndUpdateRecursive(uow, actionToUpdate.Value, existingAction, pendingConfiguration);
                    }
                }
            }

            return existingAction;
        }

        public ActionDO GetById(IUnitOfWork uow, Guid id)
        {
            return uow.ActionRepository.GetQuery().Include(i => i.ActivityTemplate).FirstOrDefault(i => i.Id == id);
        }

        public ActionDO Create(IUnitOfWork uow, int actionTemplateId, string name, string label, RouteNodeDO parentNode)
        {
            var template = uow.ActivityTemplateRepository.GetByKey(actionTemplateId);

            if (template == null)
            {
                throw new ApplicationException("Could not find ActivityTemplate.");
            }

            var action = new ActionDO
            {
                Id = Guid.NewGuid(),
                ActivityTemplate = template,
                Name = name,
                Label = label,
                CrateStorage = _crate.EmptyStorageAsStr(),
                Ordering = parentNode.ChildNodes.Count > 0 ? parentNode.ChildNodes.Max(x => x.Ordering) + 1 : 1,
                RootRouteNode = parentNode.RootRouteNode,
                Fr8Account = (parentNode.RootRouteNode != null) ? parentNode.RootRouteNode.Fr8Account : null
            };

            uow.ActionRepository.Add(action);

            parentNode.ChildNodes.Add(action);

            return action;
        }

        public async Task<RouteNodeDO> CreateAndConfigure(IUnitOfWork uow, string userId, int actionTemplateId, string name, string label = null, Guid? parentNodeId = null, bool createRoute = false)
        {
            if (parentNodeId != null && createRoute)
            {
                throw new ArgumentException("Parent node id can't be set together with create route flag");
            }

            if (parentNodeId == null && !createRoute)
            {
                throw new ArgumentException("Either Parent node id or create route flag must be set");
            }

            RouteNodeDO parentNode;
            RouteDO route = null;

            if (createRoute)
            {
                route = ObjectFactory.GetInstance<IRoute>().Create(uow, label);
                parentNode = ObjectFactory.GetInstance<ISubroute>().Create(uow, route, name + " #1");
            }
            else
            {
                parentNode = uow.RouteNodeRepository.GetByKey(parentNodeId);
            }

            var action = Create(uow, actionTemplateId, name, label, parentNode);

            uow.SaveChanges();

            await ConfigureSingleAction(uow, userId, action);

            if (createRoute)
            {
                return route;
            }

            return action;
        }

        private async Task<ActionDO> CallActionConfigure(string userId, ActionDO curActionDO)
        {
            if (curActionDO == null)
            {
                throw new ArgumentNullException("curActionDO");
            }

            var tempActionDTO = Mapper.Map<ActionDTO>(curActionDO);

            if (!_authorizationToken.ValidateAuthenticationNeeded(userId, tempActionDTO))
            {
                curActionDO = Mapper.Map<ActionDO>(tempActionDTO);

                try
                {
                    tempActionDTO = await CallTerminalActionAsync<ActionDTO>("configure", curActionDO, Guid.Empty);
                }
                catch (ArgumentException e)
                {
                    EventManager.TerminalConfigureFailed("<no terminal url>", JsonConvert.SerializeObject(curActionDO), e.Message, curActionDO.Id.ToString());
                    throw;
                }
                catch (RestfulServiceException e)
                {
                    // terminal requested token invalidation
                    if (e.StatusCode == 419)
                    {
                        _authorizationToken.InvalidateToken(userId, tempActionDTO);
                    }
                    else
                    {
                        JsonSerializerSettings settings = new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        };
                        var endpoint = (curActionDO.ActivityTemplate != null && curActionDO.ActivityTemplate.Terminal != null && curActionDO.ActivityTemplate.Terminal.Endpoint != null) ? curActionDO.ActivityTemplate.Terminal.Endpoint : "<no terminal url>";
                        EventManager.TerminalConfigureFailed(endpoint, JsonConvert.SerializeObject(curActionDO, settings), e.Message, curActionDO.Id.ToString());
                        throw;
                    }
                }
                catch (Exception e)
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects
                    };

                    var endpoint = (curActionDO.ActivityTemplate != null && curActionDO.ActivityTemplate.Terminal != null && curActionDO.ActivityTemplate.Terminal.Endpoint != null) ? curActionDO.ActivityTemplate.Terminal.Endpoint : "<no terminal url>";
                    EventManager.TerminalConfigureFailed(endpoint, JsonConvert.SerializeObject(curActionDO, settings), e.Message, curActionDO.Id.ToString());
                    throw;
                }
            }

            return Mapper.Map<ActionDO>(tempActionDTO);
        }

        private async Task<ActionDO> ConfigureSingleAction(IUnitOfWork uow, string userId, ActionDO curActionDO)
        {
            curActionDO = await CallActionConfigure(userId, curActionDO);

            UpdateActionProperties(uow, curActionDO);

            return curActionDO;
        }

        public async Task<ActionDTO> Configure(string userId, ActionDO curActionDO, bool saveResult = true)
        {
            curActionDO = await CallActionConfigure(userId, curActionDO);
            if (saveResult)
            {
                //save the received action as quickly as possible
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    curActionDO = SaveOrUpdateAction(uow, curActionDO);
                    return Mapper.Map<ActionDTO>(curActionDO);
                }
            }
            return Mapper.Map<ActionDTO>(curActionDO);
        }

        public ActionDO MapFromDTO(ActionDTO curActionDTO)
        {
            ActionDO submittedAction = AutoMapper.Mapper.Map<ActionDO>(curActionDTO);
            return SaveOrUpdateAction(submittedAction);
        }

        public void Delete(Guid id)
        {
            //we are using Kludge solution for now
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

                    using (var updater = _crate.UpdateStorage(() => currentActivity.CrateStorage))
                    {
                        bool hasChanges = false;
                        foreach (var configurationControls in updater.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>())
                        {
                            foreach (IResettable resettable in configurationControls.Controls)
                            {
                                resettable.Reset();
                                hasChanges = true;
                            }
                        }

                        if (!hasChanges)
                        {
                            updater.DiscardChanges();
                        }
                    }
                }

                _routeNode.Delete(uow, curAction);

                uow.SaveChanges();
            }
        }

        /// <summary>
        /// The method checks if the action being deleted is CurrentActivity for its ActionList. 
        /// if it is, sets CurrentActivity to the next Action, or null if it is the last action. 
        /// </summary>
        /// <param name="curActionId">Action Id</param>
        /// <param name="uow">Unit of Work</param>
        /// <returns>Returns the current action (if found) or null if not.</returns>
        //        public ActivityDO UpdateCurrentActivity(int curActionId, IUnitOfWork uow)
        //        {
        //            // Find an ActionList for which the action is set as CurrentActivity
        //            // Also, get the whole list of actions for this Action List 
        //            var curActionList = uow.ActionRepository.GetQuery().Where(al => al.Id == curActionId).Include(al => al.Activities).SingleOrDefault();
        //            if (curActionList == null) return null;
        //
        //            // Get current Action
        //            var curAction = curActionList.Activities.SingleOrDefault(a => a.Id == curActionId);
        //            if (curAction == null) return null; // Well, who knows...
        //
        //            // Get ordered list of next Activities 
        //            var activities = curActionList.Activities.Where(a => a.Ordering > curAction.Ordering).OrderBy(a => a.Ordering);
        //            
        //            curActionList.CurrentActivity = activities.FirstOrDefault();
        //
        //            return curAction;
        //        }

        public async Task PrepareToExecute(ActionDO curAction, ActionState curActionState, ContainerDO curContainerDO, IUnitOfWork uow)
        {
            EventManager.ActionStarted(curAction);

            var payload = await Run(curAction, curActionState, curContainerDO);

            if (payload != null)
            {
                using (var updater = _crate.UpdateStorage(() => curContainerDO.CrateStorage))
                {
                    updater.CrateStorage = _crate.FromDto(payload.CrateStorage);
                }
                //curContainerDO.CrateStorage = payload.CrateStorage;
            }

            uow.ActionRepository.Attach(curAction);
            uow.SaveChanges();
        }

        // Maxim Kostyrkin: this should be refactored once the TO-DO snippet below is redesigned
        public async Task<PayloadDTO> Run(ActionDO curActionDO, ActionState curActionState, ContainerDO curContainerDO)
        {
            if (curActionDO == null)
            {
                throw new ArgumentNullException("curActionDO");
            }

            try
            {
                var actionName = curActionState == ActionState.InitialRun ? "Run" : "ChildrenExecuted";
                var payloadDTO = await CallTerminalActionAsync<PayloadDTO>(actionName, curActionDO, curContainerDO.Id);
                return payloadDTO;

            }
            catch (ArgumentException e)
            {
                EventManager.TerminalRunFailed("<no terminal url>", JsonConvert.SerializeObject(curActionDO), e.Message, curActionDO.Id.ToString());
                throw;
            }
            catch (Exception e)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                };

                var endpoint = (curActionDO.ActivityTemplate != null && curActionDO.ActivityTemplate.Terminal != null && curActionDO.ActivityTemplate.Terminal.Endpoint != null) ? curActionDO.ActivityTemplate.Terminal.Endpoint : "<no terminal url>";
                EventManager.TerminalRunFailed(endpoint, JsonConvert.SerializeObject(curActionDO, settings), e.Message, curActionDO.Id.ToString());
                throw;
            }
        }


        //looks for the Configuration Controls Crate and Extracts the ManifestSchema
        public StandardConfigurationControlsCM GetControlsManifest(ActionDO curAction)
        {
            var control = _crate.GetStorage(curAction.CrateStorage).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            //            var curCrateStorage = JsonConvert.DeserializeObject<CrateStorageDTO>(curAction.CrateStorage);
            //            var curControlsCrate =
            //                _crate.GetCratesByManifestType(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME, curCrateStorage)
            //                    .FirstOrDefault();

            if (control == null)
            {
                throw new ApplicationException(string.Format("No crate found with Label == \"Configuration_Controls\" and ManifestType == \"{0}\"", CrateManifestTypes.StandardConfigurationControls));
            }


            return control;

        }

        public async Task<ActionDTO> Activate(ActionDO curActionDO)
        {
            try
            {
                //if this action contains nested actions, do not pass them to avoid 
                // circular reference error during JSON serialization (FR-1769)
                curActionDO = Mapper.Map<ActionDO>(curActionDO);
                curActionDO.ChildNodes = new List<RouteNodeDO>();

                var result = await CallTerminalActionAsync<ActionDTO>("activate", curActionDO, Guid.Empty);
                EventManager.ActionActivated(curActionDO);
                return result;
            }
            catch (ArgumentException)
            {
                EventManager.TerminalActionActivationFailed("<no terminal url>", JsonConvert.SerializeObject(curActionDO), curActionDO.Id.ToString());
                throw;
            }
            catch
            {
                EventManager.TerminalActionActivationFailed(curActionDO.ActivityTemplate.Terminal.Endpoint, JsonConvert.SerializeObject(curActionDO), curActionDO.Id.ToString());
                throw;
            }
        }

        public async Task<ActionDTO> Deactivate(ActionDO curActionDO)
        {
            return await CallTerminalActionAsync<ActionDTO>("deactivate", curActionDO, Guid.Empty);
        }

        //private Task<PayloadDTO> RunActionAsync(string actionName, ActionDO curActionDO, Guid containerId)
        //{
        //    if (actionName == null) throw new ArgumentNullException("actionName");
        //    if (curActionDO == null) throw new ArgumentNullException("curActionDO");

        //    var dto = Mapper.Map<ActionDO, ActionDTO>(curActionDO);
        //    dto.ContainerId = containerId;
        //    _authorizationToken.PrepareAuthToken(dto);

        //    EventManager.ActionDispatched(curActionDO, containerId);

        //    if (containerId != Guid.Empty)
        //    {
        //        using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //        {
        //            var containerDO = uow.ContainerRepository.GetByKey(containerId);
        //            EventManager.ContainerSent(containerDO, curActionDO);
        //            var reponse = ObjectFactory.GetInstance<ITerminalTransmitter>().CallActionAsync<PayloadDTO>(actionName, dto);
        //            EventManager.ContainerReceived(containerDO, curActionDO);
        //            return reponse;
        //        }
        //    }

        //    return ObjectFactory.GetInstance<ITerminalTransmitter>().CallActionAsync<PayloadDTO>(actionName, dto);
        //}

        private Task<TResult> CallTerminalActionAsync<TResult>(string actionName, ActionDO curActionDO, Guid containerId)
        {
            if (actionName == null) throw new ArgumentNullException("actionName");
            if (curActionDO == null) throw new ArgumentNullException("curActionDO");

            var dto = Mapper.Map<ActionDO, ActionDTO>(curActionDO);
            dto.ContainerId = containerId;
            _authorizationToken.PrepareAuthToken(dto);

            EventManager.ActionDispatched(curActionDO, containerId);

            if (containerId != Guid.Empty)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var containerDO = uow.ContainerRepository.GetByKey(containerId);
                    EventManager.ContainerSent(containerDO, curActionDO);
                    var reponse = ObjectFactory.GetInstance<ITerminalTransmitter>().CallActionAsync<TResult>(actionName, dto, containerId.ToString());
                    EventManager.ContainerReceived(containerDO, curActionDO);
                    return reponse;
                }
            }

            return ObjectFactory.GetInstance<ITerminalTransmitter>().CallActionAsync<TResult>(actionName, dto, containerId.ToString());
        }


        //        public Task<IEnumerable<T>> FindCratesByManifestType<T>(ActionDO curActionDO, GetCrateDirection direction = GetCrateDirection.None)
        //        {
        //
        //        }

        //        public async Task<IEnumerable<JObject>> FindKeysByCrateManifestType(ActionDO curActionDO, Data.Interfaces.Manifests.Manifest curSchema, string key,
        //                                                                string fieldName = "name",
        //                                                                GetCrateDirection direction = GetCrateDirection.None)
        //        {
        //            var controlsCrates = _crate.GetCratesByManifestType(curSchema.ManifestName, curActionDO.CrateStorageDTO()).ToList();
        //
        //            if (direction != GetCrateDirection.None)
        //        {
        //                var upstreamCrates = await ObjectFactory.GetInstance<IRouteNode>()
        //                    .GetCratesByDirection(curActionDO.Id, curSchema.ManifestName, direction).ConfigureAwait(false);
        //
        //                controlsCrates.AddRange(upstreamCrates);
        //            }
        //
        //            var keys = _crate.GetElementByKey(controlsCrates, key: key, keyFieldName: fieldName);
        //           return keys;
        //        }
    }
}