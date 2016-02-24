using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Hub.Managers;
using Data.Interfaces.DataTransferObjects;
using Data.Crates;
using Data.Control;
using Data.Interfaces.Manifests;
namespace terminalBaseTests.Actions
{
    public class terminalActivityMock_v1 : BaseTerminalActivity
    {
        public override Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            AddCrateMethodInvoked(curActivityDO, "Configure");
            return Task.FromResult(curActivityDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            AddCrateMethodInvoked(curActivityDO, "InitialConfigurationResponse");
            return base.InitialConfigurationResponse(curActivityDO, authTokenDO);
        }

        protected override Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            AddCrateMethodInvoked(curActivityDO, "FollowupConfigurationResponse");
            return base.FollowupConfigurationResponse(curActivityDO, authTokenDO);
        }

        public override Task<ActivityDO> Activate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            AddCrateMethodInvoked(curActivityDO, "Activate");
            return base.Activate(curActivityDO, authTokenDO);
        }

        public override Task<ActivityDO> Deactivate(ActivityDO curActivityDO)
        {
            AddCrateMethodInvoked(curActivityDO, "Deactivate");
            return base.Deactivate(curActivityDO);
        }

        public Task<ActivityDO> OtherMethod(ActivityDO curActivityDO)
        {
            AddCrateMethodInvoked(curActivityDO, "OtherMethod");
            return base.Deactivate(curActivityDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            AddCrateMethodInvoked(curActivityDO, "Run");
            var processPayload = new PayloadDTO(Guid.NewGuid());
            return await Task.FromResult(processPayload);
        }

        public async Task<PayloadDTO> ExecuteChildActivities(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            AddCrateMethodInvoked(curActivityDO, "ExecuteChildActivities");
            var processPayload = new PayloadDTO(Guid.NewGuid());
            
            return await Task.FromResult(processPayload);
        }

        private void AddCrateMethodInvoked(ActivityDO curActivityDO, string methodName)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Remove<StandardConfigurationControlsCM>();
                crateStorage.Replace(new CrateStorage(CreateControlsCrate(methodName)));
            }
        }

        private Crate CreateControlsCrate(string fieldName)
        {
            var fieldFilterPane = new TextBox
            {
                Label = fieldName,
                Name = "InvokedMethod"
            };

            return PackControlsCrate(fieldFilterPane);
        }
    }
}
