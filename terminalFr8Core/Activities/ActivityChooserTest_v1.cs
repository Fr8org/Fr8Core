using System.Threading.Tasks;
using Data.Control;
using Data.Entities;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class ActivityChooserTest_v1 : BaseTerminalActivity
    {
        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }
            else
            {
                return ConfigurationRequestType.Followup;
            }
        }

        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                AddControl(crateStorage, new ActivityChooser()
                {
                    Name = "Address",
                    Label = "Address"
                });
            }

            return Task.FromResult(curActivityDO);
        }
    }
}