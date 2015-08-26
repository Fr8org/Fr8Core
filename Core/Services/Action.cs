using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using Core.Interfaces;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.PluginRegistrations;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using Data.States;
using Data.Wrappers;
using Newtonsoft.Json;

namespace Core.Services
{
    public class Action : IAction
    {
        private readonly ISubscription _subscription;
        private IPluginRegistration _pluginRegistration;
        private IEnvelope _envelope;
        private IDocuSignTemplate _docusignTemplate;  //TODO: switch to wrappers
        private Task curAction;
        private IPluginRegistration _basePluginRegistration;

        public Action()
        {
            _subscription = ObjectFactory.GetInstance<ISubscription>();
            _pluginRegistration = ObjectFactory.GetInstance<IPluginRegistration>();
            _envelope = ObjectFactory.GetInstance<IEnvelope>();
           
            _basePluginRegistration = ObjectFactory.GetInstance<IPluginRegistration>();
        }

        public IEnumerable<TViewModel> GetAllActions<TViewModel>()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ActionRepository.GetAll().Select(Mapper.Map<TViewModel>);
            }
        }

        public IEnumerable<ActionRegistrationDO> GetAvailableActions(IDockyardAccountDO curAccount)
        {
            var plugins = _subscription.GetAuthorizedPlugins(curAccount);
            return plugins.SelectMany(p => p.AvailableActions).OrderBy(s => s.ActionType);
        }

        public bool SaveOrUpdateAction(ActionDO currentActionDo)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var existingActionDo = uow.ActionRepository.GetByKey(currentActionDo.Id);
                if (existingActionDo != null)
                {
                    existingActionDo.ActionList = currentActionDo.ActionList;
                    existingActionDo.ActionListId = currentActionDo.ActionListId;
                    existingActionDo.ActionType = currentActionDo.ActionType;
                    existingActionDo.ConfigurationSettings = currentActionDo.ConfigurationSettings;
                    existingActionDo.FieldMappingSettings = currentActionDo.FieldMappingSettings;
                    existingActionDo.ParentPluginRegistration = currentActionDo.ParentPluginRegistration;
                    existingActionDo.UserLabel = currentActionDo.UserLabel;
                }
                else
                {
                    uow.ActionRepository.Add(currentActionDo);
                }
                uow.SaveChanges();
                return true;
            }
        }

        public ActionDO GetById(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ActionRepository.GetByKey(id);
            }
        }
        public ActionDO GetConfigurationSettings(ActionRegistrationDO curActionRegistrationDO)
        {
            ActionDO curActionDO = new ActionDO();
            if (curActionRegistrationDO != null)
            {
                string pluginRegistrationName = _pluginRegistration.AssembleName(curActionRegistrationDO);
                curActionDO.ConfigurationSettings = _pluginRegistration.CallPluginRegistrationByString(pluginRegistrationName, "GetConfigurationSettings", curActionRegistrationDO);
            }
            else
                throw new ArgumentNullException("ActionRegistrationDO");
            return curActionDO;
        }

        public void Delete(int id)
        {
            ActionDO entity = new ActionDO() { Id = id };

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActionRepository.Attach(entity);
                uow.ActionRepository.Remove(entity);
                uow.SaveChanges();
            }
        }


        public async Task<string> Process(ActionDO curAction)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //if status is unstarted, change it to in-process. If status is completed or error, throw an exception.
                if (curAction.ActionState == ActionState.Unstarted)
                {
                    curAction.ActionState = ActionState.Inprocess;
                    uow.ActionRepository.Attach(curAction);
                    uow.SaveChanges();

                    EventManager.ActionStarted(curAction);
                    var pluginRegistration = BasePluginRegistration.GetPluginType(curAction);
                    Uri baseUri = new Uri(pluginRegistration.BaseUrl, UriKind.Absolute);
                    var jsonResult = await Dispatch(curAction, baseUri);

                    var jsonDeserialized = JsonConvert.DeserializeObject<Dictionary<string, ErrorDTO>>(jsonResult);
                    if (jsonDeserialized.Count == 0 || jsonDeserialized.Where(k => k.Key.ToLower().Contains("error")).Any())
                    {
                        curAction.ActionState = ActionState.Error;
                    }
                    else
                    {
                        curAction.ActionState = ActionState.Completed;
                    }

                    uow.ActionRepository.Attach(curAction);
                    uow.SaveChanges();
                }
                else
                {
                    curAction.ActionState = ActionState.Error;
                    uow.ActionRepository.Attach(curAction);
                    uow.SaveChanges();
                    throw new Exception(string.Format("Action ID: {0} status is not unstarted.", curAction.Id));
                }
            }
            return curAction.ActionState.ToString();
        }

        public async Task<string> Dispatch(ActionDO curActionDO, Uri curBaseUri)
        {
            PayloadMappingsDTO mappings;
            if (curActionDO == null)
                throw new ArgumentNullException("curAction");
            var curPluginClient = ObjectFactory.GetInstance<IPluginTransmitter>();
            curPluginClient.BaseUri = curBaseUri;
            var actionPayloadDTO = Mapper.Map<ActionPayloadDTO>(curActionDO);
            actionPayloadDTO.EnvelopeId = curActionDO.ActionList.Process.EnvelopeId; 
            
            //this is currently null because ProcessId isn't being written to ActionList.
            //that probably wasn't implemented because it doesn't actually make much sense to store a ProcessID on an ActionList
            //that's because an ActionList is essentially a template that's part of a processnodetemplate, from which N different Processes can be spawned
            //the confusion stems from design flaws that will be addressed in 921.   
            //in the short run, modify ActionList#Process to write the current processid into the ActionListDo, just to unblock this.                                                                    
            
            
            //If no existing payload, created and save it
            if (actionPayloadDTO.PayloadMappings.Count() == 0)
            {
                mappings = CreateActionPayload(curActionDO, actionPayloadDTO.EnvelopeId);
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    curActionDO.PayloadMappings = mappings.Serialize();
                    uow.SaveChanges();
                }
                actionPayloadDTO.PayloadMappings = mappings;
            }

            var jsonResult = await curPluginClient.PostActionAsync(curActionDO.ActionType, actionPayloadDTO);
            EventManager.ActionDispatched(actionPayloadDTO);
            return jsonResult;
        }

        public PayloadMappingsDTO CreateActionPayload(ActionDO curActionDO, string curEnvelopeId)
        {
            var curEnvelopeData = _envelope.GetEnvelopeData(curEnvelopeId);
            if (String.IsNullOrEmpty(curActionDO.FieldMappingSettings))
            {
                throw new InvalidOperationException("Field mappings are empty on ActionDO with id " + curActionDO.Id);
            }
            return _envelope.ExtractPayload(curActionDO.FieldMappingSettings, curEnvelopeId, curEnvelopeData);
        }


        //retrieve the list of data sources for the drop down list boxes on the left side of the field mapping pane in process builder

        public IEnumerable<string> GetFieldDataSources(ActionDO curActionDO)
        {
            _docusignTemplate = ObjectFactory.GetInstance<IDocuSignTemplate>();
            return _docusignTemplate.GetMappableSourceFields(curActionDO.DocuSignTemplateId);
        }

        //retrieve the list of data sources for the text labels on the  right side of the field mapping pane in process builder

        public Task<IEnumerable<string>> GetFieldMappingTargets(ActionDO curActionDO)
        {
            var _parentPluginRegistration = BasePluginRegistration.GetPluginType(curActionDO);
            return _parentPluginRegistration.GetFieldMappingTargets(curActionDO);
        }


    }
}