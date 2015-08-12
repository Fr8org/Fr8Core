using Core.Interfaces;
using Data.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Core.PluginRegistrations
{
    public class BasePluginRegistration : IPluginRegistration
    {
        private readonly string _AvailableActions = "";
        private readonly string _BaseUrlKey = "";
        private readonly IAction _action;

        public BasePluginRegistration(IAction action, string availableActions, string baseURLKey)
        {
            _AvailableActions = availableActions;
            _BaseUrlKey = baseURLKey;
            _action = action;
        }

        public string BaseUrl
        {
            get
            {
                return ConfigurationManager.AppSettings[_BaseUrlKey];
            }
        }

        public IEnumerable<ActionRegistrationDO> AvailableCommands
        {
            get
            {
                return GetParsedJSON();
            }
        }

        protected IEnumerable<ActionRegistrationDO> GetParsedJSON()
        {
            var result = JsonConvert.DeserializeObject<IEnumerable<ActionRegistrationDO>>(_AvailableActions, new JsonSerializerSettings());
            return result;
        }

        public virtual void RegisterActions()
        {
            IEnumerable<ActionRegistrationDO> actionList = GetParsedJSON();
            foreach (var item in actionList)
            {
                _action.Register(item.ActionType, this.GetType().Name, item.Version);
            }
        }
    }
}