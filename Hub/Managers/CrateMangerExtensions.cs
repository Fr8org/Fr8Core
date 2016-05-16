using System;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;

namespace Hub.Managers
{
    public static class CrateManagerExtensions
    {
        public static IUpdatableCrateStorage GetUpdatableStorage(this ICrateManager crateManager, ActivityDTO activity)
        {
            if (activity == null) throw new ArgumentNullException("action");
            return crateManager.UpdateStorage(() => activity.CrateStorage);
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
    }
}
