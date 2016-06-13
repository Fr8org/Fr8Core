using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;

namespace terminalDemo.Activities
{
    public class DoMath_v1 : EnhancedTerminalActivity<DoMath_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Do_Math",
            Label = "Do Math",
            Category = ActivityCategory.Processors,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string RunTimeCrateLabel = "DoMathRunTimeCrate";
        private const string ResultFieldLabel = "DoMathResult";
        

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextSource LeftArgument { get; set; }
            public DropDownList Operation { get; set; }
            public TextSource RightArgument { get; set; }

            public ActivityUi()
            {
                LeftArgument = new TextSource
                {
                    Label = "Right Argument",
                    Name = nameof(LeftArgument),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                Operation = new DropDownList
                {
                    Label = "Body",
                    Name = nameof(Operation),
                    Required = true,
                    ListItems = new List<ListItem>
                    {
                        new ListItem() { Key = "+", Selected = true, Value = "+"},
                        new ListItem() { Key = "-", Selected = true, Value = "-"},
                        new ListItem() { Key = "*", Selected = true, Value = "*"},
                        new ListItem() { Key = "/", Selected = true, Value = "/"}
                    }
                };
                RightArgument = new TextSource
                {
                    Label = "Left Argument",
                    Name = nameof(LeftArgument),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                Controls = new List<ControlDefinitionDTO> { LeftArgument, Operation, RightArgument };
            }
        }

        public DoMath_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override Task Initialize()
        {
            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RunTimeCrateLabel, true).AddField(new FieldDTO(ResultFieldLabel, AvailabilityType.RunTime));
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        protected override Task Validate()
        {
            double argumentOutput;
            var isRightArgumentValid = double.TryParse(ActivityUI.RightArgument.GetValue(Payload), out argumentOutput);
            if (!isRightArgumentValid)
            {
                ValidationManager.SetError("Invalid data entered for RightArgument", nameof(ActivityUI.RightArgument));
            }
            var isLeftArgumentValid = double.TryParse(ActivityUI.LeftArgument.GetValue(Payload), out argumentOutput);
            if (!isLeftArgumentValid)
            {
                ValidationManager.SetError("Invalid data entered for LeftArgument", nameof(ActivityUI.LeftArgument));
            }
            var operators = new char[] { '-', '*', '/', '+' };
            if (ActivityUI.Operation.selectedKey.IndexOfAny(operators) < 0)
            {
                ValidationManager.SetError("No operator was selected", nameof(ActivityUI.Operation));
            }
            return Task.FromResult(0);
        }

        public override Task Run()
        {
            var rightArgument = double.Parse(ActivityUI.RightArgument.GetValue(Payload));
            var leftArgument = double.Parse(ActivityUI.LeftArgument.GetValue(Payload));
            double result;
            switch (ActivityUI.Operation.selectedKey)
            {
                case "-":
                    result = rightArgument - leftArgument;
                    break;
                case "*":
                    result = rightArgument * leftArgument;
                    break;
                case "+":
                    result = rightArgument + leftArgument;
                    break;
                case "/":
                    result = rightArgument / leftArgument;
                    break;
                default:
                    throw new ActivityExecutionException("Unknown operator selected");
            }
            var resultField = new FieldDTO(ResultFieldLabel, result.ToString(CultureInfo.InvariantCulture), AvailabilityType.RunTime);
            var resultCrate = Crate.FromContent(RunTimeCrateLabel, new StandardPayloadDataCM(resultField));
            Payload.Add(resultCrate);
            return Task.FromResult(0);
        }
    }
}