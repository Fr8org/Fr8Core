using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;

namespace $safeprojectname$.Activities
{
    public class My_Activity_v1 : TerminalActivity<My_Activity_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "My_Activity",
            Label = "My Activity",
            Category = ActivityCategory.Processors,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string RunTimeCrateLabel = "ActivityRunTimeCrate";
        private const string ResultFieldLabel = "ActivityResult";
        

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextSource LeftArgument { get; set; }
            public DropDownList Operation { get; set; }
            public TextSource RightArgument { get; set; }

            public ActivityUi()
            {
                LeftArgument = new TextSource
                {
                    InitialLabel = "Left Argument",
                    Label = "Left Argument",
                    Name = nameof(LeftArgument),
                    Source  = new FieldSourceDTO
                    {
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                        RequestUpstream = true
                    }
                }; 
                Operation = new DropDownList
                {
                    Label = "Operation",
                    Name = nameof(Operation),
                    Required = true,
                    ListItems = new List<ListItem>
                    {
                        new ListItem() { Key = "+", Selected = true, Value = "+"},
                        new ListItem() { Key = "-", Selected = false, Value = "-"},
                        new ListItem() { Key = "*", Selected = false, Value = "*"},
                        new ListItem() { Key = "/", Selected = false, Value = "/"}
                    },
                    Value = "+",
                    selectedKey = "+"
                };
                RightArgument = new TextSource
                {
                    InitialLabel = "Right Argument",
                    Label = "Right Argument",
                    Name = nameof(RightArgument),
                    Source = new FieldSourceDTO
                    {
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                        RequestUpstream = true
                    }
                };
                Controls = new List<ControlDefinitionDTO> { LeftArgument, Operation, RightArgument };
            }
        }

        public My_Activity_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override Task Initialize()
        {
            var resultField = new FieldDTO(ResultFieldLabel, AvailabilityType.RunTime);
            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RunTimeCrateLabel, true).AddField(resultField);
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        protected override Task Validate()
        {
            if (!ActivityUI.RightArgument.HasValue)
            {
                ValidationManager.SetError("No data was entered for RightArgument", nameof(ActivityUI.RightArgument));
            }
            if (!ActivityUI.LeftArgument.HasValue)
            {
                ValidationManager.SetError("No data was entered for LeftArgument", nameof(ActivityUI.LeftArgument));
            }
            var operators = new char[] { '-', '*', '/', '+' };
            if (ActivityUI.Operation.selectedKey == null || ActivityUI.Operation.selectedKey.IndexOfAny(operators) < 0)
            {
                ValidationManager.SetError("No operator was selected", nameof(ActivityUI.Operation));
            }
            return Task.FromResult(0);
        }

        protected bool IsValid()
        {
            double argumentOutput;
            var isRightArgumentValid = double.TryParse(ActivityUI.RightArgument.GetValue(Payload), out argumentOutput);
            if (!isRightArgumentValid)
            {
                ValidationManager.SetError("Invalid data entered for RightArgument", nameof(ActivityUI.RightArgument));
                return false;
            }
            var isLeftArgumentValid = double.TryParse(ActivityUI.LeftArgument.GetValue(Payload), out argumentOutput);
            if (!isLeftArgumentValid)
            {
                ValidationManager.SetError("Invalid data entered for LeftArgument", nameof(ActivityUI.LeftArgument));
                return false;
            }
            return true;
        }

        public override Task Run()
        {
            if (!IsValid())
            {
                RaiseError("Invalid input was selected/entered", ErrorType.Generic,
                    ActivityErrorCode.DESIGN_TIME_DATA_INVALID, MyTemplate.Name, MyTemplate.Terminal.Name);

            }
            else
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
                var resultField = new KeyValueDTO(ResultFieldLabel, result.ToString(CultureInfo.InvariantCulture));
                var resultCrate = Crate.FromContent(RunTimeCrateLabel, new StandardPayloadDataCM(resultField));
                Payload.Add(resultCrate);
            }
            return Task.FromResult(0);
        }
    }
}