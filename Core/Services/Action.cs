using AutoMapper;
using Core.Enums;
using Core.Interfaces;
using Core.Managers;
using Core.Managers.APIManagers.Transmitters.Plugin;
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

namespace Core.Services
{
    public class Action : IAction
    {
        private readonly ICrateManager _crate;
        //private Task curAction;
        private readonly IPlugin _plugin;
        //private IRoute _route;
        private readonly Authorization _authorizationToken;

        private readonly IRouteNode _routeNode;

        public Action()
        {
            _authorizationToken = new Authorization();
            _plugin = ObjectFactory.GetInstance<IPlugin>();

            
            _routeNode = ObjectFactory.GetInstance<IRouteNode>();

          //  _processTemplate = ObjectFactory.GetInstance<IProcessTemplate>();
            _crate= ObjectFactory.GetInstance<ICrateManager>();
          //  _route = ObjectFactory.GetInstance<IRoute>();
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
            ActionDO existingActionDO = null;
            ActionDO curAction;

            if (submittedActionData.ActivityTemplateId == 0)
            {
                submittedActionData.ActivityTemplateId = null;
            }

            if (submittedActionData.Id > 0)
            {
                existingActionDO = uow.ActionRepository.GetByKey(submittedActionData.Id);
            }

            if (submittedActionData.IsTempId)
            {
                submittedActionData.Id = 0;
            }

            if (existingActionDO != null)
            {
                existingActionDO.ParentRouteNode = submittedActionData.ParentRouteNode;
                existingActionDO.ParentRouteNodeId = submittedActionData.ParentRouteNodeId;
                existingActionDO.ActivityTemplateId = submittedActionData.ActivityTemplateId;
                existingActionDO.Name = submittedActionData.Name;
                existingActionDO.CrateStorage = submittedActionData.CrateStorage;
                curAction = existingActionDO;
            }
            else
            {
                uow.ActionRepository.Add(submittedActionData);
                curAction = submittedActionData;
            }

            uow.SaveChanges();
            curAction.IsTempId = false;
            return curAction;
        }

        public ActionDO SaveOrUpdateAction(ActionDO submittedActionData)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return SaveOrUpdateAction(uow, submittedActionData);
            }
        }

        public ActionDO GetById(int id)
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
        public ActionDO Update(IUnitOfWork uow, ActionDO submittedActionData)
        {
            // Update properties and structure recisurively
            var existingAction = UpdateRecursive(uow, submittedActionData);

            // Change parent it it is necessary
            existingAction.ParentRouteNode = submittedActionData.ParentRouteNode;
            existingAction.ParentRouteNodeId = submittedActionData.ParentRouteNodeId;

            return existingAction;
        }

        private ActionDO UpdateRecursive(IUnitOfWork uow, ActionDO action)
        {
            var existingAction = uow.ActionRepository.GetByKey(action.Id);

            if (existingAction == null)
            {
                throw new Exception("Action was not found");
            }

            // Update properties
            existingAction.ActivityTemplateId = action.ActivityTemplateId;
            existingAction.Name = action.Name;
            existingAction.Label = action.Label;
            existingAction.CrateStorage = action.CrateStorage;

            // If existing actions has children their structure and properties
            if (action.ChildNodes != null)
            {
                // Dictionary is used to avoid O(action.ChildNodes.Count*existingAction.ChildNodes.Count) complexity when computing difference between sets. 
                // desired set of children. 
                var newChildren = action.ChildNodes.OfType<ActionDO>().ToDictionary(x => x.Id, y => y);
                // current set of children
                var currentChildren = existingAction.ChildNodes.OfType<ActionDO>().ToDictionary(x => x.Id, y => y);

                // Now we must find what child must be added to existingAction
                // Chilren to be added are difference between set newChildren and currentChildren (those elements that exist in newChildren but do not exist in currentChildren).
                foreach (var newAction in newChildren.Where(x => !currentChildren.ContainsKey(x.Key)).ToArray())
                {
                    var newChild = Update(uow, newAction.Value);
                    existingAction.ChildNodes.Add(newChild);
                }

                // Now we must find what child must be removed from existingAction
                // Chilren to be removed are difference between set currentChildren and newChildren (those elements that exist in currentChildren but do not exist in newChildren).
                foreach (var actionToRemove in currentChildren.Where(x => !newChildren.ContainsKey(x.Key)).ToArray())
                {
                    existingAction.ChildNodes.Remove(actionToRemove.Value);
                }

                // We just update those children that haven't changed (exists both in newChildren and currentChildren)
                foreach (var actionToUpdate in newChildren.Where(x => currentChildren.ContainsKey(x.Key)))
                {
                    Update(uow, actionToUpdate.Value);
                }
            }

            return existingAction;
        }


        public ActionDO GetById(IUnitOfWork uow, int id)
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
                ActivityTemplate = template,
                Name = name,
                Label = label,
                CrateStorage = JsonConvert.SerializeObject(new CrateStorageDTO())
            };

            uow.ActionRepository.Add(action);

            parentNode.ChildNodes.Add(action);

            return action;
        }

        public async Task<RouteNodeDO> CreateAndConfigure(IUnitOfWork uow, int actionTemplateId, string name, string label = null, int? parentNodeId = null, bool createRoute = false)
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
                route = ObjectFactory.GetInstance<IRoute>().Create(uow, name);
                parentNode = ObjectFactory.GetInstance<ISubroute>().Create(uow, route, name + " #1");
            }
            else
            {
                parentNode = uow.RouteNodeRepository.GetByKey(parentNodeId.Value);
            }

            var action = Create(uow, actionTemplateId, name, label, parentNode);

            uow.SaveChanges();

            var actionConfigured = (await Configure(action)).Item2;

            // Update crates on the initial action instance with those we received 
            // as a result of calling configure method. 
            action.CrateStorage = actionConfigured.CrateStorage;

            if (createRoute)
            {
                return route;
            }

            return action;
        }


        public async Task<Tuple<ActionDTO, ActionDO>> Configure(ActionDO curActionDO)
        {
            if (curActionDO == null)
                throw new ArgumentNullException("curActionDO");
            ActionDTO tempActionDTO;
            try
            {
                tempActionDTO = await CallPluginActionAsync<ActionDTO>("configure", curActionDO);
            }
            catch (ArgumentException e)
            {
                EventManager.PluginConfigureFailed("<no plugin url>", JsonConvert.SerializeObject(curActionDO), e.Message);
                throw;
            }
            catch (Exception e)
            {
                var pluginUrl = curActionDO.ActivityTemplate != null && curActionDO.ActivityTemplate.Plugin != null
                    ? curActionDO.ActivityTemplate.Plugin.Endpoint
                    : "<no plugin url>";
                EventManager.PluginConfigureFailed(pluginUrl, JsonConvert.SerializeObject(curActionDO), e.Message);
                throw;
            }

            //Plugin Configure Action Return ActionDTO
            curActionDO = Mapper.Map<ActionDO>(tempActionDTO);

            //save the received action as quickly as possible
            SaveOrUpdateAction(curActionDO);

            //Returning ActionDTO and ActionDO
            return new Tuple<ActionDTO, ActionDO>(tempActionDTO, curActionDO);
        }

        public ActionDO MapFromDTO(ActionDTO curActionDTO)
        {
            ActionDO submittedAction = AutoMapper.Mapper.Map<ActionDO>(curActionDTO);
            return SaveOrUpdateAction(submittedAction);
        }

        public void Delete(int id)
        {
            //we are using Kludge solution for now
            //https://maginot.atlassian.net/wiki/display/SH/Action+Deletion+and+Reordering
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                var curAction = uow.RouteNodeRepository.GetQuery().FirstOrDefault(al => al.Id == id);
                if (curAction == null)
                {
                    throw new InvalidOperationException("Unknown RouteNode with id: "+ id);
                }

                var downStreamActivities = _routeNode.GetDownstreamActivities(uow, curAction).OfType<ActionDO>();
                //we should clear values of configuration controls

                foreach (var downStreamActivity in downStreamActivities)
                {
                    var crateStorage = downStreamActivity.CrateStorageDTO();
                    var cratesToReset = _crate.GetCratesByManifestType(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME, crateStorage).ToList();
                    foreach (var crateDTO in cratesToReset)
                    {
                        var configurationControls = _crate.GetStandardConfigurationControls(crateDTO);
                        foreach (var controlDefinitionDTO in configurationControls.Controls)
                        {
                            (controlDefinitionDTO as IResettable).Reset();
                        }
                        crateDTO.Contents = JsonConvert.SerializeObject(configurationControls);
                    }

                    if (cratesToReset.Any())
                {
                        downStreamActivity.CrateStorage = JsonConvert.SerializeObject(crateStorage);
                    }                    
                }
                uow.RouteNodeRepository.Remove(curAction);
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

        public async Task PrepareToExecute(ActionDO curAction, ContainerDO curContainerDO, IUnitOfWork uow)
        {
                EventManager.ActionStarted(curAction);

                var payload = await Run(curAction, curContainerDO);

                if (payload != null)
                {
                    curContainerDO.CrateStorage = payload.CrateStorage;
                }

                uow.ActionRepository.Attach(curAction);
                uow.SaveChanges();
            }

        // Maxim Kostyrkin: this should be refactored once the TO-DO snippet below is redesigned
        public async Task<PayloadDTO> Run(ActionDO curActionDO, ContainerDO curContainerDO)
        {
            if (curActionDO == null)
            {
                throw new ArgumentNullException("curActionDO");
            }

            var payloadDTO = await CallPluginActionAsync<PayloadDTO>("Run", curActionDO, curContainerDO.Id);
            
            // Temporarily commented out by yakov.gnusin.
            EventManager.ActionDispatched(curActionDO, curContainerDO.Id);

            return payloadDTO;
        }


        //looks for the Configuration Controls Crate and Extracts the ManifestSchema
        public StandardConfigurationControlsCM GetControlsManifest(ActionDO curAction)
        {

            var curCrateStorage = JsonConvert.DeserializeObject<CrateStorageDTO>(curAction.CrateStorage);
            var curControlsCrate =
                _crate.GetCratesByManifestType(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME, curCrateStorage)
                    .FirstOrDefault();

            if (curControlsCrate == null || string.IsNullOrEmpty(curControlsCrate.Contents))
            {
                throw new ApplicationException(string.Format("No crate found with Label == \"Configuration_Controls\" and ManifestType == \"{0}\"", CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME));
            }


            return JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(curControlsCrate.Contents);

        }

        public async Task<ActionDTO> Activate(ActionDO curActionDO)
        {
            try
            {
                var result = await CallPluginActionAsync<ActionDTO>("activate", curActionDO);
                EventManager.ActionActivated(curActionDO);
                return result;
            }
            catch (ArgumentException)
            {
                EventManager.PluginActionActivationFailed("<no plugin url>", JsonConvert.SerializeObject(curActionDO));
                throw;
            }
            catch
            {
                EventManager.PluginActionActivationFailed(curActionDO.ActivityTemplate.Plugin.Endpoint, JsonConvert.SerializeObject(curActionDO));
                throw;
            }
        }

        public async Task<ActionDTO> Deactivate(ActionDO curActionDO)
        {
            return await CallPluginActionAsync<ActionDTO>("deactivate", curActionDO);
        }

        private Task<TResult> CallPluginActionAsync<TResult>(string actionName, ActionDO curActionDO, int processId = 0)
        {
            if (actionName == null) throw new ArgumentNullException("actionName");
            if (curActionDO == null) throw new ArgumentNullException("curActionDO");
            
            var dto = Mapper.Map<ActionDO, ActionDTO>(curActionDO);
            dto.ProcessId = processId;
            _authorizationToken.PrepareAuthToken(dto);

            return ObjectFactory.GetInstance<IPluginTransmitter>()
                .CallActionAsync<TResult>(actionName, dto);
        }


        public async Task<IEnumerable<JObject>> FindKeysByCrateManifestType(ActionDO curActionDO, Data.Interfaces.Manifests.Manifest curSchema, string key,
                                                                string fieldName = "name",
                                                                GetCrateDirection direction = GetCrateDirection.None)
        {
            var controlsCrates = _crate.GetCratesByManifestType(curSchema.ManifestName, curActionDO.CrateStorageDTO()).ToList();

            if (direction != GetCrateDirection.None)
        {
                var upstreamCrates = await ObjectFactory.GetInstance<IRouteNode>()
                    .GetCratesByDirection(curActionDO.Id, curSchema.ManifestName, direction).ConfigureAwait(false);

                controlsCrates.AddRange(upstreamCrates);
            }

            var keys = _crate.GetElementByKey(controlsCrates, key: key, keyFieldName: fieldName);
           return keys;
        }
    }
}