using System.Collections.Generic;

namespace Core.PluginRegistrations
{
    public abstract class BasePluginRegistration : IPluginRegistration
    {
        public abstract string BaseUrl { get; }

        public abstract IEnumerable<string> AvailableCommands { get; }
    }
}
