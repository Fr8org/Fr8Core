using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Infrastructure;
using Fr8Data.Manifests;
using System;

namespace Fr8Data.Managers
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
 
        public static ICrateStorage UpdateControls<TActivityUi>(this ICrateStorage crateStorage, Action<TActivityUi> action) where TActivityUi  : StandardConfigurationControlsCM, new()
        {
            if (crateStorage == null)
            {
                throw new ArgumentNullException(nameof(crateStorage));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            var crateManager = new CrateManager();
            var controlsCrate = crateStorage.FirstCrate<StandardConfigurationControlsCM>();
            var activityUi = new TActivityUi().ClonePropertiesFrom(controlsCrate.Content) as TActivityUi;
            action(activityUi);
            crateStorage.ReplaceByLabel(Crate.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray()), controlsCrate.Availability));
            return crateStorage;
        }
 
        /// <summary>
        /// Returns a copy of AcvitityUI for the given activity
        /// </summary>
        public static TActivityUi GetReadonlyActivityUi<TActivityUi>(this ICrateStorage crateStorage) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            if (crateStorage == null)
            {
                throw new ArgumentNullException(nameof(crateStorage));
            }

            return new TActivityUi().ClonePropertiesFrom(crateStorage.FirstCrateOrDefault<StandardConfigurationControlsCM>()?.Content) as TActivityUi;
        }
    }
}
