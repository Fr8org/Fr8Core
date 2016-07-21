using System;
using Fr8.Infrastructure.Data.Convertors;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Data.Managers
{
    public static class CrateManagerExtensions
    {
        public static IUpdatableCrateStorage GetUpdatableStorage(this ICrateManager crateManager, ActivityDTO activity)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));
            return crateManager.UpdateStorage(() => activity.CrateStorage);
        }

        public static IUpdatableCrateStorage GetUpdatableStorage(this ICrateManager crateManager, PayloadDTO payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            return crateManager.UpdateStorage(() => payload.CrateStorage);
        }

        public static ICrateStorage GetStorage(this ICrateManager crateManager, CrateStorageDTO crateStorageDTO)
        {
            if (crateStorageDTO == null)
            {
                return new CrateStorage();
            }

            return crateManager.FromDto(crateStorageDTO);
        }

        public static ICrateStorage GetStorage(this ICrateManager crateManager, string crateStorageRaw)
        {
            if (string.IsNullOrWhiteSpace(crateStorageRaw))
            {
                return new CrateStorage();
            }

            return crateManager.FromDto(StringToCrateStorageDTOConverter.Convert(crateStorageRaw));
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
    }
}
