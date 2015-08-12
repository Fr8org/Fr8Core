using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Core.PluginRegistrations
{
    public class BasePluginRegistration : IPluginRegistration
    {
        public string BaseUrl { get; set; }

        public IEnumerable<string> AvailableCommands { get; set; }

        public abstract IEnumerable<string> GetAvailableActions();

        public abstract JObject GetConfigurationSettings();

        public abstract IEnumerable<string> GetFieldMappingTargets(string curActionName, string configUiData);
    }
}