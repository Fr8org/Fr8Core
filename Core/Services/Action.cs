using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Core.Interfaces;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using StructureMap;
using System.Data.Entity;
using Data.Constants;
using Newtonsoft.Json;
using Data.Interfaces.ManifestSchemas;
using Utilities;
using Newtonsoft.Json.Linq;

namespace Core.Services
{
    public class Action : IAction
    {
        private ICrate _crate;
        //private Task curAction;
        private IPlugin _plugin;
        //private IProcessTemplate _processTemplate;
        private readonly AuthorizationToken _authorizationToken;

        public Action()
        {
            _authorizationToken = new AuthorizationToken();
            _plugin = ObjectFactory.GetInstance<IPlugin>();
            _crate= ObjectFactory.GetInstance<ICrate>();
          //  _processTemplate = ObjectFactory.GetInstance<IProcessTemplate>();
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
                existingActionDO.ParentActivity = submittedActionData.ParentActivity;
                existingActionDO.ParentActivityId = submittedActionData.ParentActivityId;
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
        
        public List<CrateDTO> GetCrates(ActionDO curActionDO)
        {
            return curActionDO.CrateStorageDTO().CrateDTO;
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
            catch (ArgumentException)
            {
                EventManager.PluginConfigureFailed("<no plugin url>", JsonConvert.SerializeObject(curActionDO));
                throw;
            }
            catch (Exception)
            {
                EventManager.PluginConfigureFailed(curActionDO.ActivityTemplate.Plugin.Endpoint, JsonConvert.SerializeObject(curActionDO));
                throw;
            }

            //Plugin Configure Action Return ActionDTO
            curActionDO = Mapper.Map<ActionDO>(tempActionDTO);

            //save the received action as quickly as possible
            SaveOrUpdateAction(curActionDO);

            //Returning ActionDTO
            return tempActionDTO;
        }

        public StandardConfigurationControlsMS GetConfigurationControls(ActionDO curActionDO)
        {
            var curActionDTO = Mapper.Map<ActionDTO>(curActionDO);
            var confControls = GetCratesByManifestType(MT.StandardConfigurationControls.GetEnumDisplayName(), curActionDTO.CrateStorage);
            if (confControls.Count() != 0 && confControls.Count() != 1)
                throw new ArgumentException("Expected number of CrateDTO is 0 or 1. But got '{0}'".format(confControls.Count()));
            if (confControls.Count() == 0)
                return null;
            StandardConfigurationControlsMS standardCfgControlsMs = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(confControls.First().Contents);
            return standardCfgControlsMs;
        }

        public ActionDO MapFromDTO(ActionDTO curActionDTO)
        {
            ActionDO submittedAction = AutoMapper.Mapper.Map<ActionDO>(curActionDTO);
            return SaveOrUpdateAction(submittedAction);
        }

        public void Delete(int id)
        {

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAction = uow.ActivityRepository.GetQuery().FirstOrDefault(al => al.Id == id);
                if (curAction == null) // Why we add something in Delete method?!!! (Vladimir)
                {
                    curAction = new ActivityDO { Id = id };
                    uow.ActivityRepository.Attach(curAction);
                }
                uow.ActivityRepository.Remove(curAction);
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
            
        public async Task<int> PrepareToExecute(ActionDO curAction, ProcessDO curProcessDO, IUnitOfWork uow)
        {
            //if status is unstarted, change it to in-process. If status is completed or error, throw an exception.
            if (curAction.ActionState == ActionState.Unstarted || curAction.ActionState == ActionState.InProcess)
            {
                curAction.ActionState = ActionState.InProcess;
                uow.SaveChanges();

                EventManager.ActionStarted(curAction);

                var payload = await Execute(curAction, curProcessDO);
                if (payload != null)
                {
                    curProcessDO.CrateStorage = payload.CrateStorage;
                }

                //this JSON error check is broken because it triggers on standard success messages, which look like this:
                //"{\"success\": {\"ErrorCode\": \"0\", \"StatusCode\": \"200\", \"Description\": \"\"}}"


                //check if the returned JSON is Error
                //  if (jsonResult.ToLower().Contains("error"))
                // {
                //     curAction.ActionState = ActionState.Error;
                //  }
                //   else
                //   {
                curAction.ActionState = ActionState.Active;
                //   }

                uow.ActionRepository.Attach(curAction);
                uow.SaveChanges();
            }
            else
            {
                curAction.ActionState = ActionState.Error;
                uow.ActionRepository.Attach(curAction);
                uow.SaveChanges();
                throw new Exception(string.Format("Action ID: {0} status is {1}.", curAction.Id, curAction.ActionState));
            }
            return curAction.ActionState.Value;
        }

        // Maxim Kostyrkin: this should be refactored once the TO-DO snippet below is redesigned
        public async Task<PayloadDTO> Execute(ActionDO curActionDO, ProcessDO curProcessDO)
        {
            if (curActionDO == null)
            {
                throw new ArgumentNullException("curActionDO");
            }

            var payloadDTO = await CallPluginActionAsync<PayloadDTO>("Execute", curActionDO, curProcessDO.Id);
            
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
                DockyardAccountDO curDockyardAccountDO = GetAccount(curActionDO);
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

        public async Task AuthenticateInternal(DockyardAccountDO account, PluginDO plugin,
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
            DockyardAccountDO user, PluginDO plugin)
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
        public DockyardAccountDO GetAccount(ActionDO curActionDO)
        {
            if (curActionDO.ParentActivity != null && curActionDO.ActivityTemplate.AuthenticationType == "OAuth")
            {
                // Can't follow guideline to init services inside constructor. 
                // Current implementation of ProcessTemplate and Action services are not good and are depedant on each other.
                // Initialization of services in constructor will cause stack overflow
                var processTemplate = ObjectFactory.GetInstance<IProcessTemplate>().GetProcessTemplate(curActionDO);
                return processTemplate != null ? processTemplate.DockyardAccount : null;
            }

            return null;

        }

        public void AddCrate(ActionDO curActionDO, List<CrateDTO> curCrateDTOLists)
        {
            if (curCrateDTOLists == null)
                throw new ArgumentNullException("CrateDTO is null");
            if (curActionDO == null)
                throw new ArgumentNullException("ActionDO is null");

            if (curCrateDTOLists.Count > 0)
            {
                curActionDO.UpdateCrateStorageDTO(curCrateDTOLists);
            }
        }

        public IEnumerable<CrateDTO> GetCratesByManifestType(string curManifestType, CrateStorageDTO curCrateStorageDTO)
        {
            if (String.IsNullOrEmpty(curManifestType))
                throw new ArgumentNullException("Parameter Manifest Type is empty");
            if (curCrateStorageDTO == null)
                throw new ArgumentNullException("Parameter CrateStorageDTO is null.");

            IEnumerable<CrateDTO> crateDTO = null;

            crateDTO = curCrateStorageDTO.CrateDTO.Where(crate => crate.ManifestType == curManifestType);

            return crateDTO;
        }

        public IEnumerable<CrateDTO> GetCratesByLabel(string curLabel, CrateStorageDTO curCrateStorageDTO)
        {
            if (String.IsNullOrEmpty(curLabel))
                throw new ArgumentNullException("Parameter Label is empty");
            if (curCrateStorageDTO == null)
                throw new ArgumentNullException("Parameter CrateStorageDTO is null.");

            IEnumerable<CrateDTO> crateDTOList = null;

            crateDTOList = curCrateStorageDTO.CrateDTO.Where(crate => crate.Label == curLabel);

            return crateDTOList;
        }

        //looks for the Conifiguration Controls Crate and Extracts the ManifestSchema
        public StandardConfigurationControlsMS GetControlsManifest(ActionDO curAction)
        {

            var curCrateStorage = JsonConvert.DeserializeObject<CrateStorageDTO>(curAction.CrateStorage);
            var curControlsCrate =
                GetCratesByManifestType(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME, curCrateStorage)
                    .FirstOrDefault();

            if (curControlsCrate == null || string.IsNullOrEmpty(curControlsCrate.Contents))
            {
                throw new ApplicationException(string.Format("No crate found with Label == \"Configuration_Controls\" and ManifestType == \"{0}\"", CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME));
            }


            return JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(curControlsCrate.Contents);

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
                    // Try to get owner's account for Action -> ProcessTemplate.
                    // Can't follow guideline to init services inside constructor. 
                    // Current implementation of ProcessTemplate and Action services are not good and are depedant on each other.
                    // Initialization of services in constructor will cause stack overflow
                    var processTemplate = ObjectFactory.GetInstance<IProcessTemplate>().GetProcessTemplate(action);
                    var dockyardAccount =  processTemplate != null ? processTemplate.DockyardAccount : null;
                    
                    if (dockyardAccount == null)
                    {
                        throw new ApplicationException("Could not find DockyardAccount for Action's ProcessTemplate.");
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

        public void AddCrate(ActionDO curActionDO, CrateDTO curCrateDTO)
        {
            AddCrate(curActionDO, new List<CrateDTO>() { curCrateDTO });
        }

        public void AddOrReplaceCrate(string label, ActionDO curActionDO, CrateDTO curCrateDTO)
        {
            var existingCratesWithLabelInActionDO = GetCratesByLabel(label, curActionDO.CrateStorageDTO());
            if (!existingCratesWithLabelInActionDO.Any()) // no existing crates with user provided label found, then add the crate
            {
                AddCrate(curActionDO, curCrateDTO);
            }
            else
            {
                // Remove the existing crate for this label
                _crate.RemoveCrateByLabel(curActionDO.CrateStorageDTO().CrateDTO, label);

                // Add the newly created crate for this label to action's crate storage
                AddCrate(curActionDO, curCrateDTO);
            }
        }

        public IEnumerable<JObject> FindKeysByCrateManifestType(ActionDO curActionDO, ManifestSchema curSchema, string key)
        {
           var controlsCrates = GetCratesByManifestType(curSchema.ManifestName, curActionDO.CrateStorageDTO());
           var keys = _crate.GetElementByKey(controlsCrates, key: key, keyFieldName: "name");
           return keys;
        }
    }
}