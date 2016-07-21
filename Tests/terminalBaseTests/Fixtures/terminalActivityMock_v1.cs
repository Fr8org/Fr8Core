using Data.Entities;
using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.TerminalBase.BaseClasses;

namespace terminalBaseTests.Actions
{
    public class terminalActivityMock_v1 : ExplicitTerminalActivity
    {
        public override Task Initialize()
        {
            AddCrateMethodInvoked("Initialize");
            return Task.FromResult(0);
            //return base.InitialConfigurationResponse(curActivityDO, authTokenDO);
        }

        public override Task FollowUp()
        {
            AddCrateMethodInvoked("FollowUp");
            return Task.FromResult(0);
            //return base.FollowupConfigurationResponse(curActivityDO, authTokenDO);
        }

        public override Task Activate()
        {
            AddCrateMethodInvoked("Activate");
            return base.Activate();
        }

        public override Task Deactivate()
        {
            AddCrateMethodInvoked("Deactivate");
            return base.Deactivate();
        }

        public Task OtherMethod()
        {
            AddCrateMethodInvoked("OtherMethod");
            return Task.FromResult(0);
            //return base.Deactivate(curActivityDO);
        }

        protected override ActivityTemplateDTO MyTemplate { get; }

        public override async Task Run()
        {
            AddCrateMethodInvoked("Run");
            //var processPayload = new PayloadDTO(Guid.NewGuid());
            //return await Task.FromResult(processPayload);
        }

        public async Task<PayloadDTO> ExecuteChildActivities(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            AddCrateMethodInvoked("ExecuteChildActivities");
            var processPayload = new PayloadDTO(Guid.NewGuid());
            
            return await Task.FromResult(processPayload);
        }

        private void AddCrateMethodInvoked(string methodName)
        {
            Storage.Clear();

            AddControls(new TextBox
            {
                Label = methodName,
                Name = "InvokedMethod"
            });
        }

        public terminalActivityMock_v1(ICrateManager crateManager) 
            : base(crateManager)
        {
        }
    }
}
