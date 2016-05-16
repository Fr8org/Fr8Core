using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.DataTransferObjects;
using Fr8Data.States;
using terminalFr8Core.Infrastructure;
using TerminalBase.BaseClasses;

namespace terminalFr8Core.Activities
{

    public class ManagePlan_v1 : BaseTerminalActivity
    {

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Write_To_Sql_Server",
            Label = "Write to Azure Sql Server",
            Category = ActivityCategory.Forwarders,
            Version = "1",
            MinPaneWidth = 330,
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

        public ManagePlan_v1() : base(false)
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