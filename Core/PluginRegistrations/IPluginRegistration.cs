using System.Collections.Generic;

namespace Core.PluginRegistrations
{
    public interface IPluginRegistration
    {
        string BaseUrl { get; set; }

        IEnumerable<string> AvailableCommands { get; set; }
    }
}
