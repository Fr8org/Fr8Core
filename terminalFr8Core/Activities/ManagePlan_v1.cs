using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.States;
using terminalFr8Core.Infrastructure;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

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