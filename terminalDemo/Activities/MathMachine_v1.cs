using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;

namespace terminalDemo.Activities
{
    public class MathMachine_v1 : EnhancedTerminalActivity<MathMachine_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Math_Machine",
            Label = "Math Machine",
            Category = ActivityCategory.Solution,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;


        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBlock Description { get; set; }
            public TextBox Number { get; set; }
            public TextBox Email { get; set; }
            public Button Submit { get; set; }

            public ActivityUi()
            {
                Description = new TextBlock
                {
                    Name = nameof(Description),
                    Value = "This is a tutorial solution. This solution multiplies the number you provided with 5 and sends output to your email address",
                    CssClass = "well well-lg"
                };
                Number = new TextBox
                {
                    Label = "Multiply this number by 5",
                    Name = nameof(Number),
                    Required = true
                };
                Email = new TextBox
                {
                    Label = "Send output to this email address",
                    Name = nameof(Email),
                    Required = true
                };
                Submit = new Button
                {
                    Clicked = false,
                    Name = nameof(Submit),
                    Label = "Prepare solution",
                    CssClass = "float-right mt30 btn btn-default",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfigOnClick }
                };
                Controls = new List<ControlDefinitionDTO> { Description, Number, Email, Submit };
            }
        }

        public MathMachine_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            if (ActivityUI.Submit.Clicked)
            {
                if(ActivityUI.)
            }
            return Task.FromResult(0);
        }

        public override Task Run()
        {
            return Task.FromResult(0);
        }
    }
}