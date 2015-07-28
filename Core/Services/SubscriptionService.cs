using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.PluginRegistrations;
using Data.Entities;
using Data.Interfaces;

namespace Core.Services
{
    class SubscriptionService : ISubscriptionService
    {
        public IEnumerable<IPluginRegistration> GetAuthorizedPlugins(IDockyardAccountDO account)
        {
            throw new NotImplementedException();
        }
    }
}
