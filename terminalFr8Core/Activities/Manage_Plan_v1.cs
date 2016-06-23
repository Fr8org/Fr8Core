using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using terminalFr8Core.Infrastructure;

namespace terminalFr8Core.Activities
{

    public class Manage_Plan_v1 : ExplicitTerminalActivity
    {

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Manage_Plan",
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
                    Name = "Manage_Plan",
                    Label = "Manage Plan"
                });
        }

        #endregion Configuration.

        public Manage_Plan_v1(ICrateManager crateManager)
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