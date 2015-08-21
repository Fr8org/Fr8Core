using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Core.Interfaces;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.PluginRegistrations;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Core.PluginRegistrations;
using Data.States;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.Services
{
    public class Action : IAction
    {
        private readonly ISubscription _subscription;
        IPluginRegistration _pluginRegistration;
        IEnvelope _envelope;
        private Task curAction;

        public Action()
        {
            _subscription = ObjectFactory.GetInstance<ISubscription>();
            _pluginRegistration = ObjectFactory.GetInstance<IPluginRegistration>();
            _envelope = ObjectFactory.GetInstance<IEnvelope>();
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

        public void Delete(int Id)
        {
            ActionDO entity = new ActionDO() { Id = Id };

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

        public async Task<string> Dispatch(ActionDO curActionDO, Uri baseUri)
        {
            if (curActionDO == null)
                throw new ArgumentNullException("curAction");
            var pluginClient = ObjectFactory.GetInstance<IPluginTransmitter>();
            pluginClient.BaseUri = baseUri;
            var actionPayloadDto = Mapper.Map<ActionPayloadDTO>(curActionDO);
            actionPayloadDto.PayloadMappings = CreateActionPayload(curActionDO, actionPayloadDto.EnvelopeId);
            var jsonResult = await pluginClient.PostActionAsync(curActionDO.ActionType, actionPayloadDto);
            EventManager.ActionDispatched(actionPayloadDto);
            return jsonResult;
        }

        public string CreateActionPayload(ActionDO curActionDO, string envelopeId)
        {
            var envelopeData = _envelope.GetEnvelopeData(envelopeId);
            if (String.IsNullOrEmpty(curActionDO.FieldMappingSettings))
            {
                throw new InvalidOperationException("Field mappings are empty on ActionDO with id " + curActionDO.Id);
            }
            return ParsePayloadMappings(curActionDO.FieldMappingSettings, envelopeId, envelopeData);
        }

        public string ParsePayloadMappings(string payloadMappings, string envelopeId, IList<EnvelopeDataDTO> envelopeData)
        {
            string name, value;

            JObject mappings = JObject.Parse(payloadMappings)["field_mappings"] as JObject;
            JObject payload = new JObject();

            foreach (JProperty prop in mappings.Properties())
            {
                name = prop.Name;
                value = prop.Value.ToString();
                JProperty propMapping;

                var newValue = envelopeData.Where(e => e.Name == name).Select(e => e.Value).SingleOrDefault();
                if (newValue == null)
                {
                    EventManager.DocuSignFieldMissing(envelopeId, name);
                }
                else
                {
                    propMapping = new JProperty(name, newValue);
                    payload.Add(propMapping);
                }
            }

            JObject result = new JObject(new JProperty("payload", payload));
            return result.ToString();
        }
    }
}