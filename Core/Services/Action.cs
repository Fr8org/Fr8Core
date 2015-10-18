using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI;
using AutoMapper;
using Core.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Core.Enums;
using Core.Interfaces;
using Core.Managers;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Constants;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Data.States;
using Utilities;

namespace Core.Services
{
    public class Action : IAction
    {
        private readonly ICrateManager _crate;
        //private Task curAction;
        private readonly IPlugin _plugin;
        //private IRoute _route;
        private readonly AuthorizationToken _authorizationToken;

        private readonly IRouteNode _routeNode;

        public Action()
        {
            _authorizationToken = new AuthorizationToken();
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

        public ActionDO GetById(IUnitOfWork uow, int id)
        {
            return uow.ActionRepository.GetQuery().Include(i => i.ActivityTemplate).FirstOrDefault(i => i.Id == id);
        }

        public async Task<ActionDTO> Configure(ActionDO curActionDO)
        {
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
                EventManager.PluginConfigureFailed(curActionDO.ActivityTemplate.Plugin.Endpoint, JsonConvert.SerializeObject(curActionDO), e.Message);
                throw;
            }

            //Plugin Configure Action Return ActionDTO
            curActionDO = Mapper.Map<ActionDO>(tempActionDTO);

            //save the received action as quickly as possible
            SaveOrUpdateAction(curActionDO);

            //Returning ActionDTO
            return tempActionDTO;
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

        public async Task PrepareToExecute(ActionDO curAction, ContainerDO curProcessDO, IUnitOfWork uow)
            {
                EventManager.ActionStarted(curAction);

                var payload = await Run(curAction, curProcessDO);

                if (payload != null)
                {
                    curProcessDO.CrateStorage = payload.CrateStorage;
                }

                uow.ActionRepository.Attach(curAction);
                uow.SaveChanges();
            }

        // Maxim Kostyrkin: this should be refactored once the TO-DO snippet below is redesigned
        public async Task<PayloadDTO> Run(ActionDO curActionDO, ContainerDO curProcessDO)
        {
            if (curActionDO == null)
            {
                throw new ArgumentNullException("curActionDO");
            }

            var payloadDTO = await CallPluginActionAsync<PayloadDTO>("Run", curActionDO, curProcessDO.Id);
            
            // Temporarily commented out by yakov.gnusin.
            EventManager.ActionDispatched(curActionDO, curProcessDO.Id);

            return payloadDTO;
        }

        /// <summary>
        /// Retrieve authorization token
        /// </summary>
        /// <param name="curActionDO"></param>
        /// <returns></returns>
        public string Authenticate(ActionDO curActionDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Fr8AccountDO curDockyardAccountDO = GetAccount(curActionDO);
                var curPlugin = curActionDO.ActivityTemplate.Plugin;
                string curToken = string.Empty;

                if (curDockyardAccountDO != null)
                {
                    curToken = _authorizationToken.GetToken(curDockyardAccountDO.Id, curPlugin.Id);

                    if (!string.IsNullOrEmpty(curToken))
                        return curToken;
                }

                curToken = _authorizationToken.GetPluginToken(curPlugin.Id);
                if (!string.IsNullOrEmpty(curToken))
                    return curToken;
                return _plugin.Authorize();
            }
        }

        public bool IsAuthenticated(Fr8AccountDO account, PluginDO plugin)
        {
            if (!plugin.RequiresAuthentication)
            {
                return true;
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var hasAuthToken = uow.AuthorizationTokenRepository
                    .GetQuery()
                    .Any(x => x.UserDO.Id == account.Id && x.Plugin.Id == plugin.Id);

                return hasAuthToken;
            }
        }

        public async Task AuthenticateInternal(Fr8AccountDO account, PluginDO plugin,
            string username, string password)
        {
            if (!plugin.RequiresAuthentication)
            {
                throw new ApplicationException("Plugin does not require authentication.");
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var credentialsDTO = new CredentialsDTO()
            {
                Username = username,
                Password = password
            };

            var response = await restClient.PostAsync<CredentialsDTO>(
                new Uri("http://" + plugin.Endpoint + "/actions/authenticate_internal"),
                credentialsDTO
            );

            var authTokenDTO = JsonConvert.DeserializeObject<AuthTokenDTO>(response);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var authToken = uow.AuthorizationTokenRepository
                    .FindOne(x => x.UserDO.Id == account.Id && x.Plugin.Id == plugin.Id);

                if (authTokenDTO != null)
                {
                var curPlugin = uow.PluginRepository.GetByKey(plugin.Id);
                var curAccount = uow.UserRepository.GetByKey(account.Id);

                if (authToken == null)
                {
                    authToken = new AuthorizationTokenDO()
                    {
                        Token = authTokenDTO.Token,
                        ExternalAccountId = authTokenDTO.ExternalAccountId,
                        Plugin = curPlugin,
                        UserDO = curAccount,
                        ExpiresAt = DateTime.Today.AddMonths(1)
                    };

                    uow.AuthorizationTokenRepository.Add(authToken);
                }
                else
                {
                    authToken.Token = authTokenDTO.Token;
                    authToken.ExternalAccountId = authTokenDTO.ExternalAccountId;
                }

                uow.SaveChanges();
            }
        }
        }

        public async Task AuthenticateExternal(
            PluginDO plugin,
            ExternalAuthenticationDTO externalAuthDTO)
        {
            if (!plugin.RequiresAuthentication)
            {
                throw new ApplicationException("Plugin does not require authentication.");
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var response = await restClient.PostAsync<ExternalAuthenticationDTO>(
                new Uri("http://" + plugin.Endpoint + "/actions/authenticate_external"),
                externalAuthDTO
            );

            var authTokenDTO = JsonConvert.DeserializeObject<AuthTokenDTO>(response);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var authToken = uow.AuthorizationTokenRepository
                    .FindOne(x => x.ExternalStateToken == authTokenDTO.ExternalStateToken);

                if (authToken == null)
                {
                    throw new ApplicationException("No AuthToken found with specified ExternalStateToken.");
                }

                authToken.Token = authTokenDTO.Token;
                authToken.ExternalAccountId = authTokenDTO.ExternalAccountId;
                authToken.ExternalStateToken = null;

                uow.SaveChanges();
            }
        }

        public async Task<ExternalAuthUrlDTO> GetExternalAuthUrl(
            Fr8AccountDO user, PluginDO plugin)
        {
            if (!plugin.RequiresAuthentication)
            {
                throw new ApplicationException("Plugin does not require authentication.");
            }

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var response = await restClient.PostAsync(
                new Uri("http://" + plugin.Endpoint + "/actions/auth_url")
            );

            var externalAuthUrlDTO = JsonConvert.DeserializeObject<ExternalAuthUrlDTO>(response);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var authToken = uow.AuthorizationTokenRepository
                    .FindOne(x => x.Plugin.Id == plugin.Id
                        && x.UserDO.Id == user.Id);

                if (authToken == null)
                {
                    var curPlugin = uow.PluginRepository.GetByKey(plugin.Id);
                    var curAccount = uow.UserRepository.GetByKey(user.Id);

                    authToken = new AuthorizationTokenDO()
                    {
                        UserDO = curAccount,
                        Plugin = curPlugin,
                        ExpiresAt = DateTime.Today.AddMonths(1),
                        ExternalStateToken = externalAuthUrlDTO.ExternalStateToken
                    };

                    uow.AuthorizationTokenRepository.Add(authToken);
                }
                else
                {
                    authToken.ExternalAccountId = null;
                    authToken.Token = null;
                    authToken.ExternalStateToken = externalAuthUrlDTO.ExternalStateToken;
                }

                uow.SaveChanges();
            }

            return externalAuthUrlDTO;
        }

        /// <summary>
        /// Retrieve account
        /// </summary>
        /// <param name="curActionDO"></param>
        /// <returns></returns>
        public Fr8AccountDO GetAccount(ActionDO curActionDO)
        {
            if (curActionDO.ParentRouteNode != null && curActionDO.ActivityTemplate.AuthenticationType == "OAuth")
            {
                // Can't follow guideline to init services inside constructor. 
                // Current implementation of Route and Action services are not good and are depedant on each other.
                // Initialization of services in constructor will cause stack overflow
                var route = ObjectFactory.GetInstance<IRoute>().GetRoute(curActionDO);
                return route != null ? route.Fr8Account : null;
            }

            return null;

        }       

        //looks for the Conifiguration Controls Crate and Extracts the ManifestSchema
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

        /// <summary>
        /// Prepare AuthToken for ActionDTO request message.
        /// </summary>
        private void PrepareAuthToken(ActionDTO actionDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                // Fetch ActivityTemplate.
                var activityTemplate = uow.ActivityTemplateRepository
                    .GetByKey(actionDTO.ActivityTemplateId);
                if (activityTemplate == null)
                {
                    throw new ApplicationException("Could not find ActivityTemplate.");
                }

                // Fetch Action.
                var action = uow.ActionRepository.GetByKey(actionDTO.Id);
                if (action == null)
                {
                    throw new ApplicationException("Could not find Action.");
                }

                // Try to find AuthToken if plugin requires authentication.
                if (activityTemplate.Plugin.RequiresAuthentication)
                {
                    // Try to get owner's account for Action -> Route.
                    // Can't follow guideline to init services inside constructor. 
                    // Current implementation of Route and Action services are not good and are depedant on each other.
                    // Initialization of services in constructor will cause stack overflow
                    var route = ObjectFactory.GetInstance<IRoute>().GetRoute(action);
                    var dockyardAccount = route != null ? route.Fr8Account : null;
                    
                    if (dockyardAccount == null)
                    {
                        throw new ApplicationException("Could not find DockyardAccount for Action's Route.");
                    }

                    var accountId = dockyardAccount.Id;

                    // Try to find AuthToken for specified plugin and account.
                    var authToken = uow.AuthorizationTokenRepository
                        .FindOne(x => x.Plugin.Id == activityTemplate.Plugin.Id
                            && x.UserDO.Id == accountId);

                    // If AuthToken is not empty, fill AuthToken property for ActionDTO.
                    if (authToken != null && !string.IsNullOrEmpty(authToken.Token))
                    {
                        actionDTO.AuthToken = new AuthTokenDTO()
                        {
                            Token = authToken.Token
                        };
                    }
                }
            }
        }

        private Task<TResult> CallPluginActionAsync<TResult>(string actionName, ActionDO curActionDO, int processId = 0)
        {
            if (actionName == null) throw new ArgumentNullException("actionName");
            if (curActionDO == null) throw new ArgumentNullException("curActionDO");
            
            var dto = Mapper.Map<ActionDO, ActionDTO>(curActionDO);
            dto.ProcessId = processId;
            PrepareAuthToken(dto);

            return ObjectFactory.GetInstance<IPluginTransmitter>()
                .CallActionAsync<TResult>(actionName, dto);
        }


        public async Task<IEnumerable<JObject>> FindKeysByCrateManifestType(ActionDO curActionDO, Manifest curSchema, string key,
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