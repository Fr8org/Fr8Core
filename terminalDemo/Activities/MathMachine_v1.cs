using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Services;

namespace terminalDemo.Activities
{
    public class MathMachine_v1 : TerminalActivity<MathMachine_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Math_Machine",
            Label = "Math Machine",
            Category = ActivityCategory.Solution,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[] { ActivityCategories.Solution }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string MessageBody = @"Fr8 Alert: Hey, i multiplied {0} with 5 for you. Here is the result: [DoMathResult]";
        private const string EmailSubject = @"Fr8: Math Results";

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

        public override async Task FollowUp()
        {
            if (ActivityUI.Submit.Clicked)
            {
                //Validate our inputs
                double number;
                if (string.IsNullOrEmpty(ActivityUI.Number.Value) || !double.TryParse(ActivityUI.Number.Value, out number))
                {
                    ValidationManager.SetError("Invalid or missing number", nameof(ActivityUI.Number));
                    return;
                }
                var emailChecker = new EmailAddressAttribute();
                if (string.IsNullOrEmpty(ActivityUI.Email.Value) || !emailChecker.IsValid(ActivityUI.Email.Value))
                {
                    ValidationManager.SetError("Invalid or missing email", nameof(ActivityUI.Number));
                    return;
                }
                //everything seems fine
                //lets create our children activities in parallel
                var doMathActivityTemplateTask = HubCommunicator.GetActivityTemplate("terminalDemo", "Do_Math", "1", "1");
                var buildMessageActivityTemplateTask = HubCommunicator.GetActivityTemplate("terminalFr8Core", "Build_Message", "1", "1");
                var sendEmailActivityTemplateTask = HubCommunicator.GetActivityTemplate("terminalFr8Core", "Send_Email", "1", "1");
                await Task.WhenAll(doMathActivityTemplateTask, buildMessageActivityTemplateTask, sendEmailActivityTemplateTask);
                var doMathActivityTemplate = doMathActivityTemplateTask.Result;
                var buildMessageActivityTemplate = buildMessageActivityTemplateTask.Result;
                var sendEmailActivityTemplate = sendEmailActivityTemplateTask.Result;
                
                //let's configure them all
                var doMathActivityTask = HubCommunicator.AddAndConfigureChildActivity(ActivityPayload, doMathActivityTemplate, "Do Math", "Do Math with Math Machine", 1);
                var buildMessageActivityTask = HubCommunicator.AddAndConfigureChildActivity(ActivityPayload, buildMessageActivityTemplate, "Build Message", "Build Message for Math Machine", 2);
                var sendEmailActivityTask = HubCommunicator.AddAndConfigureChildActivity(ActivityPayload, sendEmailActivityTemplate, "Send Email", "Send Math Machine output as email", 3);
                await Task.WhenAll(doMathActivityTask, buildMessageActivityTask, sendEmailActivityTask);

                var doMathActivity = doMathActivityTask.Result;
                var buildMessageActivity = buildMessageActivityTask.Result;
                var sendEmailActivity = sendEmailActivityTask.Result;

                //let's update their UI according to our inputs
                var doMathConfigurationControls = ControlHelper.GetConfigurationControls(doMathActivity.CrateStorage);
                var leftArgument = ControlHelper.GetControl<TextSource>(doMathConfigurationControls, "LeftArgument");
                var rightArgument = ControlHelper.GetControl<TextSource>(doMathConfigurationControls, "RightArgument");
                var operation = ControlHelper.GetControl<DropDownList>(doMathConfigurationControls, "Operation");
                
                //Update DoMath Activity
                leftArgument.ValueSource = TextSource.SpecificValueSource;
                leftArgument.TextValue = "5";
                rightArgument.ValueSource = TextSource.SpecificValueSource;
                rightArgument.TextValue = ActivityUI.Number.Value;
                operation.ListItems.ForEach(x => x.Selected = false);
                var multiplyOperation = operation.ListItems.Single(x => x.Key == "*");
                multiplyOperation.Selected = true;
                operation.selectedKey = "*";
                operation.Value = "*";

                //Update BuildMessage Activity
                ControlHelper.SetControlValue(buildMessageActivity, "Body", string.Format(MessageBody, ActivityUI.Number.Value));
                ControlHelper.SetControlValue(buildMessageActivity, "Name", "MathMachineMessage");

                //reconfigure BuildAMessage to publish it's message crate
                await HubCommunicator.ConfigureChildActivity(ActivityPayload, buildMessageActivity);

                //update SendEmail Activity
                var sendEmailConfigurationControls = ControlHelper.GetConfigurationControls(sendEmailActivity.CrateStorage);
                var emailAddress = ControlHelper.GetControl<TextSource>(sendEmailConfigurationControls, "EmailAddress");
                var emailSubject = ControlHelper.GetControl<TextSource>(sendEmailConfigurationControls, "EmailSubject");
                var emailBody = ControlHelper.GetControl<TextSource>(sendEmailConfigurationControls, "EmailBody");
                emailAddress.ValueSource = TextSource.SpecificValueSource;
                emailAddress.TextValue = ActivityUI.Email.Value;
                emailSubject.ValueSource = TextSource.SpecificValueSource;
                emailSubject.TextValue = EmailSubject;
                emailBody.ValueSource = TextSource.UpstreamValueSrouce;
                emailBody.SelectedItem = new FieldDTO("MathMachineMessage", AvailabilityType.RunTime);
                emailBody.selectedKey = "MathMachineMessage";
                emailBody.Value = "MathMachineMessage";
            }
        }

        public override Task Run()
        {
            return Task.FromResult(0);
        }
    }
}