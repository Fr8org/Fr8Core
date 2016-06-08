using System.Threading.Tasks;
using fr8.Infrastructure.Data.Constants;
using fr8.Infrastructure.Data.Control;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.States;
using terminalFr8Core.Infrastructure;
using TerminalBase.BaseClasses;

namespace terminalFr8Core.Activities
{

    public class ManagePlan_v1 : BaseTerminalActivity
    {

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "ManagePlan",
            Label = "Manage Plan",
            Category = ActivityCategory.Processors,
            Version = "1",
            Tags = Tags.Internal,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

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

        public ManagePlan_v1(ICrateManager crateManager)
            : base(crateManager)
        {

        }

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