using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace Hub.Managers
{
    public static class CrateMangerExtensions
    {
        public static ICrateStorageUpdater UpdateStorage(this ICrateManager crateManger, ActionDO action)
        {
            return crateManger.UpdateStorage(() => action.CrateStorage);
        }

        public static ICrateStorageUpdater UpdateStorage(this ICrateManager crateManger, ActionDTO action)
        {
            return crateManger.UpdateStorage(() => action.CrateStorage);
        }

        public static ICrateStorageUpdater UpdateStorage(this ICrateManager crateManger, PayloadDTO payload)
        {
            return crateManger.UpdateStorage(() => payload.CrateStorage);
        }

        public static CrateStorage GetStorage(this ICrateManager crateManger, ActionDO action)
        {
            return crateManger.GetStorage(action.CrateStorage);
        }
        public static CrateStorage GetStorage(this ICrateManager crateManger, ActionDTO action)
        {
            return crateManger.GetStorage(action.CrateStorage);
        }

        public static CrateStorage GetStorage(this ICrateManager crateManger, PayloadDTO payload)
        {
            return crateManger.GetStorage(payload.CrateStorage);
        }

        public static bool IsStorageEmpty(this ICrateManager crateManager, ActionDTO action)
        {
            return crateManager.IsEmptyStorage(action.CrateStorage);
        }

        public static bool IsStorageEmpty(this ICrateManager crateManager, ActionDO action)
        {
            return crateManager.IsEmptyStorage(action.CrateStorage);
        }
    }
}
