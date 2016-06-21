using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using System;

namespace terminalInstagram.Actions
{
    public class Monitor_For_New_Media_Posted_v1: TerminalActivity<Monitor_For_New_Media_Posted_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Monitor_For_New_Media_Posted",
            Label = "Monitor For New Media Posted v1",
            Category = ActivityCategory.Monitors,
            NeedsAuthentication = true,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public override Task FollowUp()
        {
            throw new NotImplementedException();
        }

        public override Task Initialize()
        {
            throw new NotImplementedException();
        }

        public override Task Run()
        {
            throw new NotImplementedException();
        }

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
                    Source = new FieldSourceDTO
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
        public Monitor_For_New_Media_Posted_v1(ICrateManager crateManager)
        : base(crateManager)
        {
        }
    }
}