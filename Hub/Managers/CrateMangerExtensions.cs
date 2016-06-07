using System;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using fr8.Infrastructure.Data.Crates;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Managers;
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

        public static IUpdatableCrateStorage GetUpdatableStorage(this ICrateManager crateManager, ActivityDTO activity)
        {
            if (activity == null) throw new ArgumentNullException("action");
            return crateManager.UpdateStorage(() => activity.CrateStorage);
        }

        public static IUpdatableCrateStorage GetUpdatableStorage(this ICrateManager crateManager, PayloadDTO payload)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            return crateManager.UpdateStorage(() => payload.CrateStorage);
        }

        public static ICrateStorage GetStorage(this ICrateManager crateManager, ActivityDO activity)
        {
           return GetStorage(crateManager, activity.CrateStorage);
        }

        public static ICrateStorage GetStorage(this ICrateManager crateManager, string crateStorageRaw)
        {
            if (string.IsNullOrWhiteSpace(crateStorageRaw))
            {
                return new CrateStorage();
            }

            return crateManager.FromDto(CrateStorageFromStringConverter.Convert(crateStorageRaw));
        }

        public static ICrateStorage GetStorage(this ICrateManager crateManager, ActivityDTO activity)
        {
            return crateManager.FromDto(activity.CrateStorage);
        }

        public static ICrateStorage GetStorage(this ICrateManager crateManager, PayloadDTO payload)
        {
            return crateManager.FromDto(payload.CrateStorage);
        }

        public static bool IsStorageEmpty(this ICrateManager crateManager, ActivityDTO activity)
        {
            return crateManager.IsEmptyStorage(activity.CrateStorage);
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
