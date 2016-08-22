//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Fr8.Infrastructure.Data.Control;
//using Fr8.Infrastructure.Data.Crates;
//using Fr8.Infrastructure.Data.DataTransferObjects;
//using Fr8.Infrastructure.Data.Managers;
//using Fr8.Infrastructure.Data.Manifests;
//using Fr8.Infrastructure.Data.States;
//using Fr8.TerminalBase.BaseClasses;
//using Newtonsoft.Json;

//namespace terminalFr8Core.Activities
//{
//    public class Show_Report_Onscreen_v1 : ExplicitTerminalActivity
//    {
//        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
//        {
//            Id = new System.Guid("7fbefa24-44cf-4220-8a3b-04f4e49bbcc8"),
//            Name = "Show_Report_Onscreen",
//            Label = "Show Report Onscreen",
//            Version = "2",
//            Category = ActivityCategory.Processors,
//            NeedsAuthentication = false,
//            MinPaneWidth = 380,
//            WebService = TerminalData.WebServiceDTO,
//            Terminal = TerminalData.TerminalDTO
//        };
//        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

//        public class ActivityUi : StandardConfigurationControlsCM
//        {
//            [JsonIgnore]
//            public UpstreamDataChooser ReportSelector { get; set; }

//            public ActivityUi()
//            {
//                Controls = new List<ControlDefinitionDTO>();

//                Controls.Add((ReportSelector = new UpstreamDataChooser
//                {
//                    Name = "ReportSelector",
//                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
//                    Label = "Display which table?"
//                }));

//                Controls.Add(new RunPlanButton());
//            }
//        }

//        public Show_Report_Onscreen_v1(ICrateManager crateManager)
//            : base(crateManager)
//        {
//        }

//        public override Task Run()
//        {
//            var actionUi = new ActivityUi();
//            actionUi.ClonePropertiesFrom(ConfigurationControls);

//            if (!string.IsNullOrWhiteSpace(actionUi.ReportSelector.SelectedLabel))
//            {
//                var reportTable = Payload.CratesOfType<StandardPayloadDataCM>().FirstOrDefault(x => x.Label == actionUi.ReportSelector.SelectedLabel);

//                if (reportTable != null)
//                {
//                    Payload.Add(Crate.FromContent("Sql Query Result", new StandardPayloadDataCM
//                    {
//                        PayloadObjects = reportTable.Content.PayloadObjects
//                    }));
//                }
//            }

//            RequestClientActivityExecution("ShowTableReport");
//            return Task.FromResult(0);
//        }

//        public override async Task Initialize()
//        {
//            Storage.Add(PackControls(new ActivityUi()));

//        }

//        public override async Task FollowUp()
//        {

//        }
//    }
//}