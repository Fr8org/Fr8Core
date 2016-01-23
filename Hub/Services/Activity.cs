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
using Microsoft.ApplicationInsights;

namespace Hub.Services
{
    public class Activity : IActivity
    {
        private readonly ICrateManager _crate;
        private readonly IAuthorization _authorizationToken;
        private readonly TelemetryClient _telemetryClient;

        private readonly IActivityTemplate _activityTemplate;
        private readonly IRouteNode _routeNode;

        public Activity()
        {
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _authorizationToken = ObjectFactory.GetInstance<IAuthorization>();
            _routeNode = ObjectFactory.GetInstance<IRouteNode>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _telemetryClient = ObjectFactory.GetInstance<TelemetryClient>();
        }

        public IEnumerable<TViewModel> GetAllActivities<TViewModel>()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ActivityRepository.GetAll().Select(Mapper.Map<TViewModel>);
            }
        }

        public ActivityDO SaveOrUpdateActivity(IUnitOfWork uow, ActivityDO submittedActivityData)
        {
            System.Diagnostics.Stopwatch stopwatch = null;
            DateTime startTime = DateTime.UtcNow;
            bool success = false;

            _telemetryClient.Context.Operation.Name = "Action#SaveOrUpdateAction";

            try
            {
                stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var activity = SaveAndUpdateRecursive(uow, submittedActivityData, null, new List<ActivityDO>());

            activity.ParentRouteNode = submittedActivityData.ParentRouteNode;
            activity.ParentRouteNodeId = submittedActivityData.ParentRouteNodeId;

            uow.SaveChanges();
                success = true;
            }
            catch
            {
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetryClient.TrackDependency("Database", "Saving Action with subactions",
                   startTime,
                   stopwatch.Elapsed,
                   success);
            }

            success = false;
            stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var result = uow.ActivityRepository.GetByKey(submittedActivityData.Id);
                success = true;
                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetryClient.TrackDependency("Database", "Getting Action by id after saving",
                   startTime,
                   stopwatch.Elapsed,
                   success);
            }
        }

        public ActivityDO SaveOrUpdateActivity(ActivityDO submittedActivityData)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return SaveOrUpdateActivity(uow, submittedActivityData);
            }
        }

        public ActivityDO GetById(Guid id)
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

        private void UpdateActionProperties(IUnitOfWork uow, ActivityDO submittedActivity)
        {
            var existingAction = uow.ActivityRepository.GetByKey(submittedActivity.Id);

            if (existingAction == null)
            {
                throw new Exception("Action was not found");
            }

            UpdateActionProperties(existingAction, submittedActivity);
            uow.SaveChanges();
        }

        private static void UpdateActionProperties(ActivityDO existingActivity, ActivityDO submittedActivity)
        {
            existingActivity.ActivityTemplateId = submittedActivity.ActivityTemplateId;
            existingActivity.Name = submittedActivity.Name;
            existingActivity.Label = submittedActivity.Label;
            existingActivity.CrateStorage = submittedActivity.CrateStorage;
            existingActivity.Ordering = submittedActivity.Ordering;
        }

        private ActivityDO SaveAndUpdateRecursive(IUnitOfWork uow, ActivityDO submittedActiviy, ActivityDO parent, List<ActivityDO> pendingConfiguration)
        {
            ActivityDO existingActivity;

            if (submittedActiviy.ActivityTemplateId == 0)
            {
                submittedActiviy.ActivityTemplateId = null;
            }

            if (submittedActiviy.IsTempId)
            {
                if (submittedActiviy.Id == Guid.Empty)
                {
                    submittedActiviy.Id = Guid.NewGuid();
                }

                existingActivity = submittedActiviy;
                submittedActiviy.IsTempId = false;

                RouteNodeDO subroute = null;

                if (parent == null)
                {
                    if (submittedActiviy.ParentRouteNodeId != null)
                    {
                        subroute = uow.RouteNodeRepository.GetByKey(submittedActiviy.ParentRouteNodeId);
                    }
                }
                else
                {
                    subroute = parent;
                }

                if (subroute == null)
                {
                    throw new Exception(string.Format("Unable to find Subroute by id = {0}", submittedActiviy.ParentRouteNodeId));
                }

                submittedActiviy.Ordering = subroute.ChildNodes.Count > 0 ? subroute.ChildNodes.Max(x => x.Ordering) + 1 : 1;

                // Add Action to repo.
                uow.ActivityRepository.Add(submittedActiviy);

                // If we have created new action add it to pending configuration list.
                pendingConfiguration.Add(submittedActiviy);

                foreach (var newAction in submittedActiviy.ChildNodes.OfType<ActivityDO>())
                {
                    newAction.ParentRouteNodeId = null;
                    newAction.ParentRouteNode = null;
                    newAction.RootRouteNodeId = submittedActiviy.RootRouteNodeId;

                    var newChild = SaveAndUpdateRecursive(uow, newAction, existingActivity, pendingConfiguration);
                    existingActivity.ChildNodes.Add(newChild);
                }
            }
            else
            {
                existingActivity = uow.ActivityRepository.GetByKey(submittedActiviy.Id);

                if (existingActivity == null)
                {
                    throw new Exception("Action was not found");
                }

                // Update properties
                UpdateActionProperties(existingActivity, submittedActiviy);

                // Sync nested action structure
                if (submittedActiviy.ChildNodes != null)
                {
                    // Dictionary is used to avoid O(action.ChildNodes.Count*existingAction.ChildNodes.Count) complexity when computing difference between sets. 
                    // desired set of children. 
                    var newChildren = submittedActiviy.ChildNodes.OfType<ActivityDO>().Where(x => !x.IsTempId).ToDictionary(x => x.Id, y => y);
                    // current set of children
                    var currentChildren = existingActivity.ChildNodes.OfType<ActivityDO>().ToDictionary(x => x.Id, y => y);

                    // Now we must find what child must be added to existingAction
                    // Chilren to be added are difference between set newChildren and currentChildren (those elements that exist in newChildren but do not exist in currentChildren).
                    foreach (var newAction in submittedActiviy.ChildNodes.OfType<ActivityDO>().Where(x => x.IsTempId || !currentChildren.ContainsKey(x.Id)).ToArray())
                    {
                        newAction.ParentRouteNodeId = null;
                        newAction.ParentRouteNode = null;
                        newAction.RootRouteNodeId = existingActivity.RootRouteNodeId;

                        var newChild = SaveAndUpdateRecursive(uow, newAction, existingActivity, pendingConfiguration);
                        existingActivity.ChildNodes.Add(newChild);
                    }



                    // Now we must find what child must be removed from existingAction
                    // Chilren to be removed are difference between set currentChildren and newChildren (those elements that exist in currentChildren but do not exist in newChildren).
                    foreach (var actionToRemove in currentChildren.Where(x => !newChildren.ContainsKey(x.Key)).ToArray())
                    {
                        existingActivity.ChildNodes.Remove(actionToRemove.Value);
                        //i (bahadir) commented out this line. currently our deletion mechanism already removes this action from it's parent
                        //TODO talk to Vladimir about this
                        //    _routeNode.Delete(uow, actionToRemove.Value);
                    }
                    // We just update those children that haven't changed (exists both in newChildren and currentChildren)
                    foreach (var actionToUpdate in newChildren.Where(x => !x.Value.IsTempId && currentChildren.ContainsKey(x.Key)))
                    {
                        SaveAndUpdateRecursive(uow, actionToUpdate.Value, existingActivity, pendingConfiguration);
                    }
                }
            }

            return existingActivity;
        }

        public ActivityDO GetById(IUnitOfWork uow, Guid id)
        {
            return uow.ActivityRepository.GetQuery().FirstOrDefault(i => i.Id == id);
        }

        public ActivityDO Create(IUnitOfWork uow, int actionTemplateId, string name, string label, RouteNodeDO parentNode, Guid? AuthorizationTokenId = null)
        {
            var activity = new ActivityDO
            {
                Id = Guid.NewGuid(),
                ActivityTemplateId =  actionTemplateId,
                Name = name,
                Label = label,
                CrateStorage = _crate.EmptyStorageAsStr(),
                Ordering = parentNode.ChildNodes.Count > 0 ? parentNode.ChildNodes.Max(x => x.Ordering) + 1 : 1,
                RootRouteNode = parentNode.RootRouteNode,
                AuthorizationTokenId = AuthorizationTokenId
            };

            uow.ActivityRepository.Add(activity);

            parentNode.ChildNodes.Add(activity);

            return activity;
        }

        public async Task<RouteNodeDO> CreateAndConfigure(IUnitOfWork uow, string userId, int actionTemplateId, string name, string label = null, Guid? parentNodeId = null, bool createRoute = false, Guid? authorizationTokenId = null)
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

            var activity = Create(uow, actionTemplateId, name, label, parentNode, authorizationTokenId);

            uow.SaveChanges();

            await ConfigureSingleAction(uow, userId, activity);

            if (createRoute)
            {
                return route;
            }

            return activity;
        }

        private async Task<ActivityDO> CallActionConfigure(string userId, ActivityDO curActivityDO)
        {
            if (curActivityDO == null)
            {
                throw new ArgumentNullException("curActivityDO");
            }

            var tempActionDTO = Mapper.Map<ActivityDTO>(curActivityDO);

            if (!_authorizationToken.ValidateAuthenticationNeeded(userId, tempActionDTO))
            {
                curActivityDO = Mapper.Map<ActivityDO>(tempActionDTO);

                try
                {
                    tempActionDTO = await CallTerminalActionAsync<ActivityDTO>("configure", curActivityDO, Guid.Empty);
                }
                catch (ArgumentException e)
                {
                    EventManager.TerminalConfigureFailed("<no terminal url>", JsonConvert.SerializeObject(curActivityDO), e.Message, curActivityDO.Id.ToString());
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
                        var endpoint = _activityTemplate.GetTerminalUrl(curActivityDO.ActivityTemplateId) ?? "<no terminal url>";
                        EventManager.TerminalConfigureFailed(endpoint, JsonConvert.SerializeObject(curActivityDO, settings), e.Message, curActivityDO.Id.ToString());
                        throw;
                    }
                }
                catch (Exception e)
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects
                    };

                    var endpoint = _activityTemplate.GetTerminalUrl(curActivityDO.ActivityTemplateId) ?? "<no terminal url>";
                    EventManager.TerminalConfigureFailed(endpoint, JsonConvert.SerializeObject(curActivityDO, settings), e.Message, curActivityDO.Id.ToString());
                    throw;
                }
            }

            return Mapper.Map<ActivityDO>(tempActionDTO);
        }

        private async Task<ActivityDO> ConfigureSingleAction(IUnitOfWork uow, string userId, ActivityDO curActivityDO)
        {
            curActivityDO = await CallActionConfigure(userId, curActivityDO);

            UpdateActionProperties(uow, curActivityDO);

            return curActivityDO;
        }

        public async Task<ActivityDTO> Configure(string userId, ActivityDO curActivityDO, bool saveResult = true)
        {
            curActivityDO = await CallActionConfigure(userId, curActivityDO);
            if (saveResult)
            {
                //save the received action as quickly as possible
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    curActivityDO = SaveOrUpdateActivity(uow, curActivityDO);
                    return Mapper.Map<ActivityDTO>(curActivityDO);
                }
            }
            return Mapper.Map<ActivityDTO>(curActivityDO);
        }

        public ActivityDO MapFromDTO(ActivityDTO curActivityDTO)
        {
            ActivityDO submittedActivity = AutoMapper.Mapper.Map<ActivityDO>(curActivityDTO);
            return SaveOrUpdateActivity(submittedActivity);
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

                var downStreamActivities = _routeNode.GetDownstreamActivities(uow, curAction).OfType<ActivityDO>();
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

        public async Task PrepareToExecute(ActivityDO curActivity, ActionState curActionState, ContainerDO curContainerDO, IUnitOfWork uow)
        {
            EventManager.ActionStarted(curActivity);

            var payload = await Run(curActivity, curActionState, curContainerDO);

            if (payload != null)
            {
                using (var updater = _crate.UpdateStorage(() => curContainerDO.CrateStorage))
                {
                    updater.CrateStorage = _crate.FromDto(payload.CrateStorage);
                }
                //curContainerDO.CrateStorage = payload.CrateStorage;
            }

            uow.ActivityRepository.Attach(curActivity);
            uow.SaveChanges();
        }

        // Maxim Kostyrkin: this should be refactored once the TO-DO snippet below is redesigned
        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, ActionState curActionState, ContainerDO curContainerDO)
        {
            if (curActivityDO == null)
            {
                throw new ArgumentNullException("curActivityDO");
            }

            try
            {
                var actionName = curActionState == ActionState.InitialRun ? "Run" : "ChildrenExecuted";
                var payloadDTO = await CallTerminalActionAsync<PayloadDTO>(actionName, curActivityDO, curContainerDO.Id);
                return payloadDTO;

            }
            catch (ArgumentException e)
            {
                EventManager.TerminalRunFailed("<no terminal url>", JsonConvert.SerializeObject(curActivityDO), e.Message, curActivityDO.Id.ToString());
                throw;
            }
            catch (Exception e)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                };

                var endpoint = _activityTemplate.GetTerminalUrl(curActivityDO.ActivityTemplateId) ?? "<no terminal url>";
                EventManager.TerminalRunFailed(endpoint, JsonConvert.SerializeObject(curActivityDO, settings), e.Message, curActivityDO.Id.ToString());
                throw;
            }
        }


        //looks for the Configuration Controls Crate and Extracts the ManifestSchema
        public StandardConfigurationControlsCM GetControlsManifest(ActivityDO curActivity)
        {
            var control = _crate.GetStorage(curActivity.CrateStorage).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
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

        public async Task<ActivityDTO> Activate(ActivityDO curActivityDO)
        {
            try
            {
                //if this action contains nested actions, do not pass them to avoid 
                // circular reference error during JSON serialization (FR-1769)
                curActivityDO = Mapper.Map<ActivityDO>(curActivityDO);
                curActivityDO.ChildNodes = new List<RouteNodeDO>();

                var result = await CallTerminalActionAsync<ActivityDTO>("activate", curActivityDO, Guid.Empty);
                EventManager.ActionActivated(curActivityDO);
                return result;
            }
            catch (ArgumentException)
            {
                EventManager.TerminalActionActivationFailed("<no terminal url>", JsonConvert.SerializeObject(curActivityDO), curActivityDO.Id.ToString());
                throw;
            }
            catch
            {
                EventManager.TerminalActionActivationFailed(_activityTemplate.GetTerminalUrl(curActivityDO.ActivityTemplateId) ?? "<no terminal url>", JsonConvert.SerializeObject(curActivityDO), curActivityDO.Id.ToString());
                throw;
            }
        }

        public async Task<ActivityDTO> Deactivate(ActivityDO curActivityDO)
        {
            return await CallTerminalActionAsync<ActivityDTO>("deactivate", curActivityDO, Guid.Empty);
        }

        //private Task<PayloadDTO> RunActionAsync(string actionName, ActionDO curActivityDO, Guid containerId)
        //{
        //    if (actionName == null) throw new ArgumentNullException("actionName");
        //    if (curActivityDO == null) throw new ArgumentNullException("curActivityDO");

        //    var dto = Mapper.Map<ActionDO, ActionDTO>(curActivityDO);
        //    dto.ContainerId = containerId;
        //    _authorizationToken.PrepareAuthToken(dto);

        //    EventManager.ActionDispatched(curActivityDO, containerId);

        //    if (containerId != Guid.Empty)
        //    {
        //        using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //        {
        //            var containerDO = uow.ContainerRepository.GetByKey(containerId);
        //            EventManager.ContainerSent(containerDO, curActivityDO);
        //            var reponse = ObjectFactory.GetInstance<ITerminalTransmitter>().CallActionAsync<PayloadDTO>(actionName, dto);
        //            EventManager.ContainerReceived(containerDO, curActivityDO);
        //            return reponse;
        //        }
        //    }

        //    return ObjectFactory.GetInstance<ITerminalTransmitter>().CallActionAsync<PayloadDTO>(actionName, dto);
        //}

        private Task<TResult> CallTerminalActionAsync<TResult>(string activityName, ActivityDO curActivityDO, Guid containerId)
        {
            if (activityName == null) throw new ArgumentNullException("activityName");
            if (curActivityDO == null) throw new ArgumentNullException("curActivityDO");

            var dto = Mapper.Map<ActivityDO, ActivityDTO>(curActivityDO);
            dto.ContainerId = containerId;
            _authorizationToken.PrepareAuthToken(dto);

            EventManager.ActionDispatched(curActivityDO, containerId);

            if (containerId != Guid.Empty)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var containerDO = uow.ContainerRepository.GetByKey(containerId);
                    EventManager.ContainerSent(containerDO, curActivityDO);
                    var reponse = ObjectFactory.GetInstance<ITerminalTransmitter>().CallActionAsync<TResult>(activityName, dto, containerId.ToString());
                    EventManager.ContainerReceived(containerDO, curActivityDO);
                    return reponse;
                }
            }

            return ObjectFactory.GetInstance<ITerminalTransmitter>().CallActionAsync<TResult>(activityName, dto, containerId.ToString());
        }


        //        public Task<IEnumerable<T>> FindCratesByManifestType<T>(ActionDO curActivityDO, GetCrateDirection direction = GetCrateDirection.None)
        //        {
        //
        //        }

        //        public async Task<IEnumerable<JObject>> FindKeysByCrateManifestType(ActionDO curActivityDO, Data.Interfaces.Manifests.Manifest curSchema, string key,
        //                                                                string fieldName = "name",
        //                                                                GetCrateDirection direction = GetCrateDirection.None)
        //        {
        //            var controlsCrates = _crate.GetCratesByManifestType(curSchema.ManifestName, curActivityDO.CrateStorageDTO()).ToList();
        //
        //            if (direction != GetCrateDirection.None)
        //        {
        //                var upstreamCrates = await ObjectFactory.GetInstance<IRouteNode>()
        //                    .GetCratesByDirection(curActivityDO.Id, curSchema.ManifestName, direction).ConfigureAwait(false);
        //
        //                controlsCrates.AddRange(upstreamCrates);
        //            }
        //
        //            var keys = _crate.GetElementByKey(controlsCrates, key: key, keyFieldName: fieldName);
        //           return keys;
        //        }
    }
}