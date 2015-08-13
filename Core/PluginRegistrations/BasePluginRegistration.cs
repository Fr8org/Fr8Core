using Core.Interfaces;
using Data.Entities;
using Newtonsoft.Json;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Configuration;
using Utilities;

namespace Core.PluginRegistrations
{
    public class BasePluginRegistration : IPluginRegistration
    {
        private readonly string availableActions = "";
        private readonly string baseUrl = "";
        private readonly IAction _action;

        public BasePluginRegistration(string curAvailableActions, string curBaseURL)
        {
            availableActions = curAvailableActions;
            baseUrl = curBaseURL;
            _action = ObjectFactory.GetInstance<IAction>();
        }

        public string BaseUrl
        {
            get
            {
                return baseUrl;
            }
        }

        public IEnumerable<ActionRegistrationDO> AvailableCommands
        {
            get
            {
                var result = JsonConvert.DeserializeObject<IEnumerable<ActionRegistrationDO>>(availableActions, new JsonSerializerSettings());
                return result;
            }
        }

        public void RegisterActions()
        {
            IEnumerable<ActionRegistrationDO> curAvailableCommands = this.AvailableCommands;
            foreach (var action in curAvailableCommands)
            {
                _action.Register(action.ActionType, this.GetType().Name, action.Version);
            }
        }
    }
}