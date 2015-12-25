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
namespace terminalTests.Actions
{
    public class terminalActionMock_v1 : BaseTerminalAction
    {
        public override Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            AddCrateMethodInvoked(curActionDO, "Configure");
            return base.Configure(curActionDO, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            AddCrateMethodInvoked(curActionDO, "InitialConfigurationResponse");
            return base.InitialConfigurationResponse(curActionDO, authTokenDO);
        }

        protected override Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            AddCrateMethodInvoked(curActionDO, "FollowupConfigurationResponse");
            return base.FollowupConfigurationResponse(curActionDO, authTokenDO);
        }

        public override Task<ActionDO> Activate(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            AddCrateMethodInvoked(curActionDO, "Activate");
            return base.Activate(curActionDO, authTokenDO);
        }

        public override Task<ActionDO> Deactivate(ActionDO curActionDO)
        {
            AddCrateMethodInvoked(curActionDO, "Deactivate");
            return base.Deactivate(curActionDO);
        }

        public Task<ActionDO> OtherMethod(ActionDO curActionDO)
        {
            AddCrateMethodInvoked(curActionDO, "OtherMethod");
            return base.Deactivate(curActionDO);
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            AddCrateMethodInvoked(curActionDO, "Run");
            var processPayload = new PayloadDTO(Guid.NewGuid());
            return await Task.FromResult(processPayload);
        }

        public async Task<PayloadDTO> ChildrenExecuted(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            AddCrateMethodInvoked(curActionDO, "ChildrenExecuted");
            var processPayload = new PayloadDTO(Guid.NewGuid());
            
            return await Task.FromResult(processPayload);
        }

        private void AddCrateMethodInvoked(ActionDO curActionDO, string methodName)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage = new CrateStorage(CreateControlsCrate(methodName));
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
