using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Core.PluginRegistrations
{
    public abstract class BasePluginRegistration : IPluginRegistration
    {
        public abstract string BaseUrl { get; }

        public abstract IEnumerable<string> AvailableCommands { get; }

        public abstract IEnumerable<string> GetAvailableActions();

        public abstract JObject GetConfigurationSettings();

        public abstract IEnumerable<string> GetFieldMappingTargets(string curActionName, string configUiData);
    }
}