using System.Collections.Generic;
using Core.PluginRegistrations;
using Data.Entities;
using Data.Interfaces;

namespace Core.Interfaces
{
    public interface ISubscription
    {
        IEnumerable<IPluginRegistration> GetAuthorizedPlugins(IDockyardAccountDO account);
    }
}