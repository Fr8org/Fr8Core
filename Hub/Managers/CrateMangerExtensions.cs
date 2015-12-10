using System;
using Data.Crates;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;

namespace Hub.Managers
{
    public static class CrateMangerExtensions
    {
        public static ICrateStorageUpdater UpdateStorage(this ICrateManager crateManger, ActionDO action)
        {
            if (action == null) throw new ArgumentNullException("action");
            return crateManger.UpdateStorage(() => action.CrateStorage);
        }

        public static ICrateStorageUpdater UpdateStorage(this ICrateManager crateManger, ActionDTO action)
        {
            if (action == null) throw new ArgumentNullException("action");
            return crateManger.UpdateStorage(() => action.CrateStorage);
        }

        public static ICrateStorageUpdater UpdateStorage(this ICrateManager crateManger, PayloadDTO payload)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            return crateManger.UpdateStorage(() => payload.CrateStorage);
        }

        public static CrateStorage GetStorage(this ICrateManager crateManger, ActionDO action)
        {
           return GetStorage(crateManger, action.CrateStorage);
        }

        public static CrateStorage GetStorage(this ICrateManager crateManger, string crateStorageRaw)
        {
            if (string.IsNullOrWhiteSpace(crateStorageRaw))
            {
                return new CrateStorage();
            }

            return crateManger.FromDto(CrateStorageFromStringConverter.Convert(crateStorageRaw));
        }

        public static CrateStorage GetStorage(this ICrateManager crateManger, ActionDTO action)
        {
            return crateManger.FromDto(action.CrateStorage);
        }

        public static CrateStorage GetStorage(this ICrateManager crateManger, PayloadDTO payload)
        {
            return crateManger.FromDto(payload.CrateStorage);
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
