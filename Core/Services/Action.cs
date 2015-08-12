using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Core.PluginRegistrations;

namespace Core.Services
{
    public class Action : IAction
    {
        private readonly ISubscription _subscription;

        public Action()
        {
            _subscription = ObjectFactory.GetInstance<ISubscription>();
        }

        public IEnumerable<TViewModel> GetAllActions<TViewModel>()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ActionRepository.GetAll().Select(Mapper.Map<TViewModel>);
            }
        }

        public IEnumerable<string> GetAvailableActions(IDockyardAccountDO curAccount)
        {
            var plugins = _subscription.GetAuthorizedPlugins(curAccount);
            return plugins.SelectMany(p => p.AvailableCommands).OrderBy(s => s, StringComparer.OrdinalIgnoreCase);
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

        public ActionDO GetConfigurationSettings(ActionRegistrationDO curActionRegistrationDO)
        {
            ActionDO curActionDO = new ActionDO();
            ActionRegistration curActionRegistration = new ActionRegistration();
            IPluginRegistration curBasePluginRegistration = new BasePluginRegistration();
            if(curActionRegistrationDO != null)
            {
                string pluginRegistrationName = curActionRegistration.AssemblePluginRegistrationName(curActionRegistrationDO);
                curActionDO.ConfigurationSettings = curBasePluginRegistration.InvokeMethod(pluginRegistrationName, "GetConfigurationSettings", curActionRegistrationDO);
            }
            else
                throw new ArgumentNullException("ActionRegistrationDO");
            return curActionDO;
        }
    }
}