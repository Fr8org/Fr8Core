using System;
using Data.Entities;
using Fr8Data.Crates;
using Fr8Data.Managers;


namespace Hub.Managers
{
    public static class CrateManagerExtensions
    {
        public static IUpdatableCrateStorage GetUpdatableStorage(this ICrateManager crateManager, ActivityDO activity)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));
            return crateManager.UpdateStorage(() => activity.CrateStorage);
        }

        public static ICrateStorage GetStorage(this ICrateManager crateManager, ActivityDO activity)
        {
            return crateManager.GetStorage(activity.CrateStorage);
        }
    }
}
