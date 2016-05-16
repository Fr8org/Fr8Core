using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Activities
{
    public class Show_Report_Onscreen_v1 : BaseTerminalActivity
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

        public class ActivityUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public UpstreamDataChooser ReportSelector { get; set; }

            public ActivityUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add((ReportSelector = new UpstreamDataChooser
                {
                    Name = "ReportSelector",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Label = "Display which table?"
                }));

                Controls.Add(new RunPlanButton());
            }
        }


        private async Task<Crate> FindTables()
        {
            var fields = new List<FieldDTO>();
            foreach (var table in (await GetCratesByDirection<FieldDescriptionsCM>(CrateDirection.Upstream))
                                             .Select(x => x.Content)
                                             .SelectMany(x => x.Fields)
                                             .Where(x => x.Availability == AvailabilityType.RunTime && x.Value == "Table"))
            {
                fields.Add(new FieldDTO(table.Key, table.Key));    
            }

            return Crate.FromContent("Upstream Crate Label List", new FieldDescriptionsCM(fields));
        }


        public Show_Report_Onscreen_v1() : base(false)
        {
        }

        public override Task Run()
        {
            var actionUi = new ActivityUi();
            actionUi.ClonePropertiesFrom(ConfigurationControls);

            if (!string.IsNullOrWhiteSpace(actionUi.ReportSelector.SelectedLabel))
            {
                var reportTable = Payload.CratesOfType<StandardPayloadDataCM>().FirstOrDefault(x => x.Label == actionUi.ReportSelector.SelectedLabel);

                if (reportTable != null)
                {
                    Payload.Add(Crate.FromContent("Sql Query Result", new StandardPayloadDataCM
                    {
                        PayloadObjects = reportTable.Content.PayloadObjects
                    }));
                }
            }

            ExecuteClientActivity("ShowTableReport");
            return Task.FromResult(0);
        }

        public override async Task Initialize()
        {
            Storage.Add(PackControls(new ActivityUi()));
            Storage.Add(await FindTables());
        }

        public override async Task FollowUp()
        {
            Storage.ReplaceByLabel(await FindTables());
        }
    }
}