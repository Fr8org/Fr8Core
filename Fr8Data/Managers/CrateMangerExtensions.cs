using System;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Infrastructure;
using Fr8Data.Manifests;

namespace Fr8Data.Managers
{
    public static class CrateManagerExtensions
    {
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
 
        private static ActivityDTO UpdateControls<TActivityUi>(this ActivityDTO activityDTO, Action<TActivityUi> action) where TActivityUi  : StandardConfigurationControlsCM, new()
        {
            if (activityDTO == null)
            {
                throw new ArgumentNullException(nameof(activityDTO));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            var crateManager = new CrateManager();
            using (var storage = crateManager.GetUpdatableStorage(activityDTO))
            {
                var controlsCrate = storage.FirstCrate<StandardConfigurationControlsCM>();
                var activityUi = new TActivityUi().ClonePropertiesFrom(controlsCrate.Content) as TActivityUi;
                action(activityUi);
                storage.ReplaceByLabel(Crate.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray()), controlsCrate.Availability));
            }
            return activityDTO;
        }
 
        /// <summary>
        /// Returns a copy of AcvitityUI for the given activity
        /// </summary>
        private static TActivityUi GetReadonlyActivityUi<TActivityUi>(this ActivityDTO activityDTO) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            if (activityDTO == null)
            {
                throw new ArgumentNullException(nameof(activityDTO));
            }
            var crateManager = new CrateManager();

            var storage = crateManager.GetStorage(activityDTO);
            return new TActivityUi().ClonePropertiesFrom(storage.FirstCrateOrDefault<StandardConfigurationControlsCM>()?.Content) as TActivityUi;
        }
    }
}
