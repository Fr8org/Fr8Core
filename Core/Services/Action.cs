using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using Core.Interfaces;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.PluginRegistrations;
using Data.Entities;
using Data.Interfaces;
using StructureMap;

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

        public void Process(ActionDO curAction)
        {
            Dispatch(curAction);
        }

        public void Dispatch(ActionDO curAction)
        {
            if (curAction == null)
                throw new ArgumentNullException("curAction");
            var pluginRegistrationType = Type.GetType(curAction.ParentPluginRegistration);
            if (pluginRegistrationType == null)
                throw new ArgumentException(string.Format("Can't find plugin registration type: {0}", curAction.ParentPluginRegistration), "curAction");
            var pluginRegistration = Activator.CreateInstance(pluginRegistrationType) as IPluginRegistration;
            if (pluginRegistration == null)
                throw new ArgumentException(string.Format("Can't find a valid plugin registration type: {0}", curAction.ParentPluginRegistration), "curAction");
            var pluginClient = ObjectFactory.GetInstance<IPluginClient>(); 
            pluginClient.BaseUri = new Uri(pluginRegistration.BaseUrl, UriKind.Absolute);
            var action = Regex.Replace(curAction.ActionType, @"\s", "_");
            var requestUri = new Uri(string.Format("/actions/{0}", action));
        }
    }
}