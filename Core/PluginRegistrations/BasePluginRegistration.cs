using Core.Interfaces;
using Data.Entities;
using Newtonsoft.Json;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Configuration;
using Utilities;
using Newtonsoft.Json.Linq;

namespace Core.PluginRegistrations
{
    public abstract class BasePluginRegistration : IPluginRegistration
    {
        private readonly string availableActions = "";
        private readonly string baseUrl = "";
        private readonly IAction _action;

        public abstract IEnumerable<string> AvailableCommands { get; }
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

            set { }
        }

        public IEnumerable<ActionRegistrationDO> AvailableCommands
        {
            get
            {
                var result = JsonConvert.DeserializeObject<IEnumerable<ActionRegistrationDO>>(availableActions, new JsonSerializerSettings());
                return result;
            }
        }
        public virtual void RegisterActions()
        {
            IEnumerable<ActionRegistrationDO> curAvailableCommands = this.AvailableCommands;
            foreach (var action in curAvailableCommands)
            {
                _action.Register(action.ActionType, this.GetType().Name, action.Version);
            }
        }
		
		public abstract IEnumerable<string> GetAvailableActions();

        public abstract JObject GetConfigurationSettings();

        public abstract IEnumerable<string> GetFieldMappingTargets(string curActionName, string configUiData);

    }
}