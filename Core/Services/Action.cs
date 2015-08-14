using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using Data.Interfaces.DataTransferObjects;

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

        public IEnumerable<ActionRegistrationDO> GetAvailableActions(IDockyardAccountDO curAccount)
        {
            var plugins = _subscription.GetAuthorizedPlugins(curAccount);
            return plugins.SelectMany(p => p.AvailableCommands).OrderBy(s => s.ActionType);
        }

        public void Register(string ActionType, string PluginRegistration, string Version)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (!uow.ActionRegistrationRepository.GetQuery().Where(a => a.ActionType == ActionType 
                    && a.Version == Version && a.ParentPluginRegistration == PluginRegistration).Any())
                {
                    ActionRegistrationDO actionRegistrationDO = new ActionRegistrationDO(ActionType, 
                                                                    PluginRegistration, 
                                                                    Version);
                    uow.SaveChanges();
                }
            }
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
    }
}