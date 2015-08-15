using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Interfaces;
using Data.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;

namespace Core.PluginRegistrations
{
    public abstract class BasePluginRegistration : IPluginRegistration
    {
        private readonly string availableActions;
        private readonly string baseUrl;
        private readonly IAction _action;

        protected BasePluginRegistration(string curAvailableActions, string curBaseUrl)
        {
            AutoMapperBootStrapper.ConfigureAutoMapper();

            availableActions = curAvailableActions;
            baseUrl = curBaseUrl;
            _action = ObjectFactory.GetInstance<IAction>();
        }

        public string BaseUrl
        {
            get { return baseUrl; }

            set { }
        }

        public IEnumerable<ActionRegistrationDO> AvailableActions
        {
            get
            {
                return JsonConvert.DeserializeObject<IEnumerable<ActionRegistrationDO>>(availableActions,
                    new JsonSerializerSettings());
            }
        }

        public virtual void RegisterActions()
        {
            IEnumerable<ActionRegistrationDO> curAvailableActions = AvailableActions;
            foreach (var action in curAvailableActions)
            {
                _action.Register(action.ActionType, GetType().Name, action.Version);
            }
        }

        public abstract JObject GetConfigurationSettings();

        public abstract Task<IEnumerable<string>> GetFieldMappingTargets(ActionDO curAction);
    }
}