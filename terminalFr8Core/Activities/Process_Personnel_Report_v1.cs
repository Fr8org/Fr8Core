using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalFr8Core.Activities
{
    public class Process_Personnel_Report_v1 : BaseTerminalActivity
    {
        private Crate CreateControlsCrate()
        {
            var infobox = new TextBlock()
            {
                Value = "This activity doesn't require configuration"
            };

            return PackControlsCrate(infobox);
        }

        protected override ConfigurationRequestType GetConfigurationRequestType()
        {
            return ConfigurationRequestType.Initial;
        }

        public Process_Personnel_Report_v1() : base(false)
        {
        }

        protected override ActivityTemplateDTO MyTemplate { get; }
        public override async Task Run()
        {
            var dspayloadDTO = Payload.FirstOrDefault(a => a.Label == "DocuSign Envelope Data");
            var dspayload = dspayloadDTO.Get<StandardPayloadDataCM>().AllValues();
            var payload = new StandardTableDataCM
            {
                FirstRowHeaders = true,
                Table = new List<TableRowDTO>
                {
                    new TableRowDTO()
                    {
                        Row = new List<TableCellDTO>()
                        {
                            new TableCellDTO() {Cell = new FieldDTO("Date", "Date")},
                            new TableCellDTO() {Cell = new FieldDTO("Volunteer", "Volunteer")},
                            new TableCellDTO() {Cell = new FieldDTO("Present", "Present")},
                            new TableCellDTO() {Cell = new FieldDTO("Zombies Neutralized", "Zombies Neutralized")},
                        },
                    }
                }
            };
            var people_count = dspayload.Count(a => a.Key.StartsWith("Zombies"));

            for (int i = 0; i < people_count; i++)
            {
                string volunteer_name = dspayload.FirstOrDefault(a => a.Key.StartsWith("Name " + i)).Value;
                string zombie_count = dspayload.FirstOrDefault(a => a.Key.StartsWith("Zombies " + i)).Value;
                string present = dspayload.FirstOrDefault(a => a.Key.StartsWith("Present " + i)).Value;

                payload.Table.Add(new TableRowDTO()
                {
                    Row = new List<TableCellDTO>()
                    {
                        new TableCellDTO() { Cell = new FieldDTO("Date",  DateTime.Now.ToString("MM/dd/yyyy"))},
                        new TableCellDTO() { Cell = new FieldDTO("Volunteer", volunteer_name)},
                        new TableCellDTO() { Cell = new FieldDTO("Present", present)},
                        new TableCellDTO() { Cell = new FieldDTO("Zombies Neutralized", zombie_count)}
                    }
                });
            }

            var crate = Crate.FromContent("Personnel Report", payload, AvailabilityType.RunTime);
            Payload.Add(crate);
            Success();
        }

        public override Task Initialize()
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();
            Storage.Add(configurationControlsCrate);
            var availableRunTimeCrates = Crate.FromContent("Available Run Time Crates", new CrateDescriptionCM(
                new CrateDescriptionDTO
                {
                    ManifestType = MT.StandardTableData.GetEnumDisplayName(),
                    Label = "Personnel Report",
                    ManifestId = (int)MT.DocuSignEnvelope,
                    ProducedBy = "Query_DocuSign_v1"
                }), AvailabilityType.RunTime);

            Storage.Add(availableRunTimeCrates);
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}