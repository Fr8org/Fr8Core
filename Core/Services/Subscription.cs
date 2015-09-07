using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.PluginRegistrations;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;

namespace Core.Services
{
    class Subscription : ISubscription
    {

        
        public IEnumerable<IPluginRegistration> GetAuthorizedPlugins(IDockyardAccountDO account)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            return account.Subscriptions
                .Where(s => s.AccessLevel == AccessLevel.User || s.AccessLevel == AccessLevel.Admin)
                .Select(s => ObjectFactory.GetNamedInstance<IPluginRegistration>(s.PluginRegistration.Name))
                .Where(pr => pr != null)
                .ToList();
        }
    }
}
