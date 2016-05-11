using System;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalFr8Core.Infrastructure;

namespace terminalFr8Core.Actions
{

    public class ManagePlan_v1 : BaseTerminalActivity

    {
        private readonly FindObjectHelper _findObjectHelper = new FindObjectHelper();


        #region Configuration.

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
                AddRunNowButton(crateStorage);
            }

            return Task.FromResult(curActivityDO);

        }

        private void AddRunNowButton(ICrateStorage crateStorage)
        {
            AddControl(crateStorage,
                new RunPlanButton()
                {
                    Name = "RunPlan",
                    Label = "Run Plan",
                });

            AddControl(crateStorage,
                new ControlDefinitionDTO(ControlTypes.ManagePlan)
                {
                    Name = "ManagePlan",
                    Label = "Manage Plan"
                });
        }

        #endregion Configuration.


        #region Execution.

        public async Task<PayloadDTO> Run(ActivityDO curActionDTO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActionDTO, containerId));
        }

        #endregion Execution.
    }
}