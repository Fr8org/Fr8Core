using System.Collections.Generic;

namespace Core.PluginRegistrations
{
    public class BasePluginRegistration : IPluginRegistration
    {
        public string BaseUrl { get; set; }

        public IEnumerable<string> AvailableCommands { get; set; }
    }
}