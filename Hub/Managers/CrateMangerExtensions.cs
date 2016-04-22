using System;
using Data.Crates;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Data.Interfaces.Manifests;

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

        public static ActivityDTO UpdateControls<TActivityUi>(this ActivityDTO activity, Action<TActivityUi> action) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            var crateManager = new CrateManager();
            using (var storage = crateManager.GetUpdatableStorage(activity))
            {
                var controlsCrate = storage.FirstCrate<StandardConfigurationControlsCM>();
                var activityUi = new TActivityUi().ClonePropertiesFrom(controlsCrate.Content) as TActivityUi;
                action(activityUi);
                storage.ReplaceByLabel(Crate.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray()), controlsCrate.Availability));
            }
            return activity;
        }

        public static ActivityDO UpdateControls<TActivityUi>(this ActivityDO activity, Action<TActivityUi> action) where TActivityUi : StandardConfigurationControlsCM, new()
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            var crateManager = new CrateManager();
            using (var storage = crateManager.GetUpdatableStorage(activity))
            {
                var controlsCrate = storage.FirstCrate<StandardConfigurationControlsCM>();
                var activityUi = new TActivityUi().ClonePropertiesFrom(controlsCrate.Content) as TActivityUi;
                action(activityUi);
                storage.ReplaceByLabel(Crate.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray()), controlsCrate.Availability));
            }
            return activity;
        }
    }
}
