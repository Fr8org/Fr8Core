using Data.Entities;
using System.Collections.Generic;

namespace Core.PluginRegistrations
{
    public interface IPluginRegistration
    {
        string BaseUrl { get; }

        IEnumerable<ActionRegistrationDO> AvailableCommands { get; }

        void RegisterActions();
    }
}
