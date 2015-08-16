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

namespace Core.Services
{
    public class Action : IAction
    {
        private readonly ISubscription _subscription;
        IPluginRegistration _pluginRegistration;

        public Action()
        {
            _subscription = ObjectFactory.GetInstance<ISubscription>();
            _pluginRegistration = ObjectFactory.GetInstance<IPluginRegistration>();
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
            if(curActionRegistrationDO != null)
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

        public async Task Process(ActionDO curAction)
        {
            await Dispatch(curAction);
        }

        public async Task Dispatch(ActionDO curAction)
        {
            if (curAction == null)
                throw new ArgumentNullException("curAction");
            var pluginRegistrationType = Type.GetType(curAction.ParentPluginRegistration);
            if (pluginRegistrationType == null)
                throw new ArgumentException(string.Format("Can't find plugin registration type: {0}", curAction.ParentPluginRegistration), "curAction");
            var pluginRegistration = Activator.CreateInstance(pluginRegistrationType) as IPluginRegistration;
            if (pluginRegistration == null)
                throw new ArgumentException(string.Format("Can't find a valid plugin registration type: {0}", curAction.ParentPluginRegistration), "curAction");
            var curActionDTO = Mapper.Map<ActionDTO>(curAction);
            var pluginClient = ObjectFactory.GetInstance<IPluginClient>(); 
            pluginClient.BaseUri = new Uri(pluginRegistration.BaseUrl, UriKind.Absolute);
            await pluginClient.PostActionAsync(curAction.ActionType, curActionDTO);
            EventManager.ActionDispatched(curAction);
        }
    }
}