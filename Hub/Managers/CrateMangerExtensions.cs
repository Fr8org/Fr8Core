using System;
using Data.Crates;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;

namespace Hub.Managers
{
    public static class CrateManagerExtensions
    {
        public static ICrateStorageUpdater UpdateStorage(this ICrateManager crateManager, ActionDO action)
        {
            if (action == null) throw new ArgumentNullException("action");
            return crateManager.UpdateStorage(() => action.CrateStorage);
        }

        public static ICrateStorageUpdater UpdateStorage(this ICrateManager crateManager, ActionDTO action)
        {
            if (action == null) throw new ArgumentNullException("action");
            return crateManager.UpdateStorage(() => action.CrateStorage);
        }

        public static ICrateStorageUpdater UpdateStorage(this ICrateManager crateManager, PayloadDTO payload)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            return crateManager.UpdateStorage(() => payload.CrateStorage);
        }

        public static CrateStorage GetStorage(this ICrateManager crateManager, ActionDO action)
        {
           return GetStorage(crateManager, action.CrateStorage);
        }

        public static CrateStorage GetStorage(this ICrateManager crateManager, string crateStorageRaw)
        {
            if (string.IsNullOrWhiteSpace(crateStorageRaw))
            {
                return new CrateStorage();
            }

            return crateManager.FromDto(CrateStorageFromStringConverter.Convert(crateStorageRaw));
        }

        public static CrateStorage GetStorage(this ICrateManager crateManager, ActionDTO action)
        {
            return crateManager.FromDto(action.CrateStorage);
        }

        public static CrateStorage GetStorage(this ICrateManager crateManager, PayloadDTO payload)
        {
            return crateManager.FromDto(payload.CrateStorage);
        }

        public static bool IsStorageEmpty(this ICrateManager crateManager, ActionDTO action)
        {
            return crateManager.IsEmptyStorage(action.CrateStorage);
        }

        public static bool IsStorageEmpty(this ICrateManager crateManager, ActionDO action)
        {
            if (string.IsNullOrWhiteSpace(action.CrateStorage))
            {
                return true;
            }

            var proxy = JsonConvert.DeserializeObject<CrateStorageDTO>(action.CrateStorage);
            
            if (proxy.Crates == null)
            {
                return true;
            }

            return proxy.Crates.Length == 0;
        }
    }
}
