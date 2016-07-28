using System;
using Data.Entities;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Newtonsoft.Json;

namespace Hub.Managers
{
    public static class CrateManagerExtensions
    {
        public static IUpdatableCrateStorage GetUpdatableStorage(this ICrateManager crateManager, ActivityDO activity)
        {
            if (activity == null) throw new ArgumentNullException("activity");
            return crateManager.UpdateStorage(() => activity.CrateStorage);
        }
        
        public static ICrateStorage GetStorage(this ICrateManager crateManager, ActivityDO activity)
        {
           return crateManager.GetStorage(activity.CrateStorage);
        }
        
        public static bool IsStorageEmpty(this ICrateManager crateManager, ActivityDO activity)
        {
            if (string.IsNullOrWhiteSpace(activity.CrateStorage))
            {
                return true;
            }

            var proxy = JsonConvert.DeserializeObject<CrateStorageDTO>(activity.CrateStorage);
            
            if (proxy.Crates == null)
            {
                return true;
            }

            return proxy.Crates.Length == 0;
        }
    }
}
