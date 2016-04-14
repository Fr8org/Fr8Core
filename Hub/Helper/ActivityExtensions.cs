using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using System;

namespace Data.Utility
{
    public static class ActivityExtensions
    {
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
    }
}
