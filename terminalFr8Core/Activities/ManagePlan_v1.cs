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

        private void AddRunNowButton()
        {
            AddControl(
                new RunPlanButton()
                {
                    Name = "RunPlan",
                    Label = "Run Plan",
                });

            AddControl(
                new ControlDefinitionDTO(ControlTypes.ManagePlan)
                {
                    Name = "ManagePlan",
                    Label = "Manage Plan"
                });
        }

        #endregion Configuration.

        public ManagePlan_v1() : base(false)
        {

        }

        protected override ActivityTemplateDTO MyTemplate { get; }
        public override Task Run()
        {
            Success();
            return Task.FromResult(0);
        }

        public override Task Initialize()
        {
            AddRunNowButton();
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}