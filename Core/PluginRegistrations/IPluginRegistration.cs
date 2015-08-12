using Data.Entities;
using System.Collections.Generic;

namespace Core.PluginRegistrations
{
    public interface IPluginRegistration
    {
        string BaseUrl { get; }

        IEnumerable<string> AvailableCommands { get; }

        string InvokeMethod(string typeName, string methodName, ActionRegistrationDO curActionRegistrationDO);
    }
}
