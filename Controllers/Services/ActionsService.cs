using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using Web.ViewModels;

namespace Web.Controllers.Services
{
    public interface IActionsService
    {
        IEnumerable<ActionVM> GetAllActions();
        IEnumerable<ActionListVM> GetAllActionLists();
        IEnumerable<string> GetAvailableActions(IDockyardAccountDO curAccount);
        bool SaveOrUpdateAction(ActionVM action);
    }

    public class ActionsService : IActionsService
    {
        private readonly ISubscriptionService _subscriptionService;

        public ActionsService()
            : this(ObjectFactory.GetInstance<ISubscriptionService>())
        {
            
        }

        public ActionsService(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        public IEnumerable<ActionVM> GetAllActions()
        {
            var items = new List<ActionVM>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var actions = uow.ActionRepository.GetAll();
                items.AddRange(actions.Select(Mapper.Map<ActionVM>));
            }

            return items;
        }

        public IEnumerable<ActionListVM> GetAllActionLists()
        {
            var items = new List<ActionListVM>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var actionLists = uow.ActionListRepository.GetAll();
                items.AddRange(actionLists.Select(Mapper.Map<ActionListVM>));
            }

            return items;
        }

        public IEnumerable<string> GetAvailableActions(IDockyardAccountDO curAccount)
        {
            var plugins = _subscriptionService.GetAuthorizedPlugins(curAccount);
            return plugins.SelectMany(p => p.AvailableCommands).OrderBy(s => s, StringComparer.OrdinalIgnoreCase);
        }
        public bool SaveOrUpdateAction(ActionVM submittedAction)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var currentActionDo = Mapper.Map<ActionDO>(submittedAction);
                var existingActionDo = uow.ActionRepository.GetByKey(submittedAction.Id);
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

                try
                {
                    uow.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}