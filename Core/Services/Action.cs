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

        //public void Register(string ActionType, string PluginRegistration, string Version)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        if (!uow.ActionRegistrationRepository.GetQuery().Where(a => a.ActionType == ActionType 
        //            && a.Version == Version && a.ParentPluginRegistration == PluginRegistration).Any())
        //        {
        //            ActionRegistrationDO actionRegistrationDO = new ActionRegistrationDO(ActionType, 
        //                                                            PluginRegistration, 
        //                                                            Version);
        //            uow.SaveChanges();
        //        }
        //    }
        //}

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
            if(curActionRegistrationDO != null)
            {
                string pluginRegistrationName = _pluginRegistration.AssembleName(curActionRegistrationDO);
                curActionDO.ConfigurationSettings = _pluginRegistration.CallPluginRegistrationByString(pluginRegistrationName, "GetConfigurationSettings", curActionRegistrationDO);
            }
            else
                throw new ArgumentNullException("ActionRegistrationDO");
            return curActionDO;
        }
    }
}