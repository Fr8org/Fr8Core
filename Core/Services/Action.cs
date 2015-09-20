using System;
using System.Collections.Generic;
using System.Linq;
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
using Data.Wrappers;
using StructureMap;
using System.Data.Entity;
using Newtonsoft.Json;

namespace Core.Services
{
    public class Action : IAction
    {
        private IEnvelope _envelope;
        private IAction _action;
        private IDocuSignTemplate _docusignTemplate; //TODO: switch to wrappers
        private Task curAction;
        private IPlugin _plugin;
        private readonly AuthorizationToken _authorizationToken;

        public Action()
        {
            _authorizationToken = new AuthorizationToken();
            _plugin = ObjectFactory.GetInstance<IPlugin>();
            
        }

        public IEnumerable<TViewModel> GetAllActions<TViewModel>()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ActionRepository.GetAll().Select(Mapper.Map<TViewModel>);
            }
        }

        public ActionDO SaveOrUpdateAction(ActionDO submittedActionData)
        {
            ActionDO existingActionDO = null;
            ActionDO curAction;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (submittedActionData.ActivityTemplateId == 0)
                    submittedActionData.ActivityTemplateId = null;

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
                return curAction;
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
                return uow.ActionRepository.GetQuery().Include(i => i.ActivityTemplate).Where(i => i.Id == id).Select(s => s).FirstOrDefault();
            }
        }

        public CrateStorageDTO Configure(ActionDO curActionDO)
        {
            ActivityTemplateDO curActivityTemplate;

            if (curActionDO != null && curActionDO.ActivityTemplateId != 0)
            {
                //fetch this Action's ActivityTemplate
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    curActivityTemplate = uow.ActivityTemplateRepository.GetByKey(curActionDO.ActivityTemplateId);

                    if (curActivityTemplate != null)
                    {
                        //convert the Action to a DTO in preparation for serialization and POST to the plugin
                        var curActionDTO = Mapper.Map<ActionDTO>(curActionDO);

                        //convert the ActivityTemplate to a DTO as well
                        ActivityTemplateDTO curActivityTemplateDTO = Mapper.Map<ActivityTemplateDTO>(curActivityTemplate);
                        curActionDTO.ActivityTemplate = curActivityTemplateDTO;

                        // prepare the current plugin URL
                        // TODO: Add logic to use https:// for production

                        string curPluginUrl = "http://" + curActivityTemplate.Plugin.Endpoint + "/actions/configure/";

                        var restClient = new RestfulServiceClient();
                        string pluginConfigurationCrateListJSON;
                        try
                        {
                            pluginConfigurationCrateListJSON =
                                restClient.PostAsync(new Uri(curPluginUrl, UriKind.Absolute), curActionDTO).Result;
                        }
                        catch (Exception)
                        {
                            EventManager.PluginConfigureFailed(curPluginUrl, JsonConvert.SerializeObject(curActionDTO));
                            throw;
                        }

                        var configurationCrates = JsonConvert.DeserializeObject<CrateStorageDTO>(pluginConfigurationCrateListJSON);                        
                        
                        //replace the old CrateStorage with the new CrateStorage
                        //this feels a little clumsy and dangerous (what if something changes in the action's crate storage while this plugin is sitting on its data?)
                        //we probably will need a complex mechanism that looks at each crate by GUID
                        curActionDTO.CrateStorage = configurationCrates;
                        curActionDO = Mapper.Map<ActionDO>(curActionDTO);

                        //save the received action as quickly as possible
                        SaveOrUpdateAction(curActionDO);
                        return curActionDO.CrateStorageDTO();  
                    }

                    else
                    {
                        throw new ArgumentNullException("ActivityTemplateDO");
                    }
                }
            }
            else
            {
                throw new ArgumentNullException("ActivityTemplateDO");
            }
        }

        public void Delete(int id)
        {
            var entity = new ActionDO { Id = id };

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActionRepository.Attach(entity);
                uow.ActionRepository.Remove(entity);
                uow.SaveChanges();
            }
        }

        public async Task<int> Process(ActionDO curAction, ProcessDO curProcessDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //if status is unstarted, change it to in-process. If status is completed or error, throw an exception.
                if (curAction.ActionState == ActionState.Unstarted || curAction.ActionState == ActionState.InProcess)
                {
                    curAction.ActionState = ActionState.InProcess;
                    uow.ActionRepository.Attach(curAction);
                    uow.SaveChanges();

                    EventManager.ActionStarted(curAction);

                    var jsonResult = await Execute(curAction, curProcessDO);

                    //this JSON error check is broken because it triggers on standard success messages, which look like this:
                    //"{\"success\": {\"ErrorCode\": \"0\", \"StatusCode\": \"200\", \"Description\": \"\"}}"


                    //check if the returned JSON is Error
                    //  if (jsonResult.ToLower().Contains("error"))
                    // {
                    //     curAction.ActionState = ActionState.Error;
                    //  }
                    //   else
                    //   {
                    curAction.ActionState = ActionState.Completed;
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
            }
            return curAction.ActionState.Value;
        }

        public async Task<string> Execute(ActionDO curActionDO, ProcessDO curProcessDO)
        {
            if (curActionDO == null)
                throw new ArgumentNullException("curActionDO");

            var curActionDTO = Mapper.Map<ActionDTO>(curActionDO);
            var curPayloadDTO = new PayloadDTO(curProcessDO.CrateStorage, curProcessDO.Id);

            //TODO: The plugin transmitter Post Async to get Payload DTO is depriciated. This logic has to be discussed and changed.
            var curPluginClient = ObjectFactory.GetInstance<IPluginTransmitter>();
            
            //TODO : Cut base Url from PluginDO.Endpoint

            curPluginClient.BaseUri = new Uri(curActionDO.ActivityTemplate.Plugin.Endpoint);

            var jsonResult = await curPluginClient.PostActionAsync(curActionDO.Name, curActionDTO, curPayloadDTO);
            EventManager.ActionDispatched(curActionDTO);

            return jsonResult;
        }

        /// <summary>
        /// Retrieve the list of data sources for the drop down list boxes on the left side of the field mapping pane in process builder
        /// </summary>
        public IEnumerable<string> GetFieldDataSources(IUnitOfWork uow, ActionDO curActionDO)
        {
            DocuSignTemplateSubscriptionDO curDocuSignSubscription = null;

            if (curActionDO.ParentActivity != null)
            {
                // Try to get ProcessTemplate.Id from relation chain
                // Action -> ActionList -> ProcessNodeTemplate -> ProcessTemplate.
                var curProcessTemplateId = ((ActionListDO)curActionDO.ParentActivity)
                    .ProcessNodeTemplate
                    .ProcessTemplate
                    .Id;

                // Try to get DocuSignSubscription related to current ProcessTemplate.Id.
                curDocuSignSubscription = uow.ExternalEventSubscriptionRepository
                    .GetQuery()
                    .OfType<DocuSignTemplateSubscriptionDO>()
                    .FirstOrDefault(x => x.DocuSignProcessTemplateId == curProcessTemplateId);
            }

            // Return list of mappable source fields, in case we fetched DocuSignSubscription object.
            if (curDocuSignSubscription != null)
            {
                _docusignTemplate = ObjectFactory.GetInstance<IDocuSignTemplate>();
                var curMappableSourceFields = _docusignTemplate
                    .GetMappableSourceFields(curDocuSignSubscription.DocuSignTemplateId);

                return curMappableSourceFields;
            }
            // Return empty list in other case.
            else
            {
                return Enumerable.Empty<string>();
            }
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


        /// <summary>
        /// Retrieve account
        /// </summary>
        /// <param name="curActionDO"></param>
        /// <returns></returns>
        public DockyardAccountDO GetAccount(ActionDO curActionDO)
        {
            if (curActionDO.ParentActivity != null
                && curActionDO.ActivityTemplate.AuthenticationType == "OAuth")
            {
                ActionListDO curActionListDO = (ActionListDO)curActionDO.ParentActivity;

                return curActionListDO
                    .Process
                    .ProcessTemplate
                    .DockyardAccount;
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
    }
}