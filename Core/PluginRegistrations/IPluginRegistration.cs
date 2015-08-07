using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Core.PluginRegistrations
{
    public interface IPluginRegistration
    {
        string BaseUrl { get; }

        IEnumerable<string> AvailableCommands { get; }

        IEnumerable<string> GetAvailableActions();

        JObject GetConfigurationSettings();

        IEnumerable<string> GetFieldMappingTargets(string curActionName, string configUiData);
    }
}