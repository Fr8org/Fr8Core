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
using TerminalBase.Infrastructure;
using TerminalBase.BaseClasses;
using Utilities;

namespace terminalFr8Core.Actions
{
    public class Process_Personnel_Report_v1 : BaseTerminalActivity
    {
        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);

            var manager = CrateManager.FromDto(payloadCrates.CrateStorage);
            var dspayloadDTO = manager.Where(a => a.Label == "DocuSign Envelope Data").FirstOrDefault();
            var dspayload = dspayloadDTO.Get<StandardPayloadDataCM>().AllValues();



            var payload = new StandardTableDataCM();

            payload.FirstRowHeaders = true;
            payload.Table = new List<TableRowDTO>();
            payload.Table.Add(new TableRowDTO()
            {
                Row = new List<TableCellDTO>()
                { new TableCellDTO() { Cell = new FieldDTO("Date", "Date")},
                new TableCellDTO() { Cell = new FieldDTO("Volunteer", "Volunteer")},
                new TableCellDTO() { Cell = new FieldDTO("Present", "Present")},
                new TableCellDTO() { Cell = new FieldDTO("Zombies Neutralized", "Zombies Neutralized")},},

            });

            var people_count = dspayload.Where(a => a.Key.StartsWith("Zombies")).Count();

            for (int i = 0; i < people_count; i++)
            {
                string volunteer_name = dspayload.Where(a => a.Key.StartsWith("Name " + i)).FirstOrDefault().Value;
                string zombie_count = dspayload.Where(a => a.Key.StartsWith("Zombies " + i)).FirstOrDefault().Value;
                string present = dspayload.Where(a => a.Key.StartsWith("Present " + i)).FirstOrDefault().Value;

                payload.Table.Add(new TableRowDTO()
                {
                    Row = new List<TableCellDTO>()
                { new TableCellDTO() { Cell = new FieldDTO("Date",  DateTime.Now.ToString("MM/dd/yyyy"))},
                new TableCellDTO() { Cell = new FieldDTO("Volunteer", volunteer_name)},
                new TableCellDTO() { Cell = new FieldDTO("Present", present)},
                new TableCellDTO() { Cell = new FieldDTO("Zombies Neutralized", zombie_count)},},

                });
            }

            var crate = Crate.FromContent("Personnel Report", payload, AvailabilityType.RunTime);

            using (var updater = CrateManager.GetUpdatableStorage(payloadCrates))
            {
                updater.Add(crate);
            }

            return Success(payloadCrates);
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
                var availableRunTimeCrates = Fr8Data.Crates.Crate.FromContent("Available Run Time Crates", new CrateDescriptionCM(
                   new CrateDescriptionDTO
                   {
                       ManifestType = MT.StandardTableData.GetEnumDisplayName(),
                       Label = "Personnel Report",
                       ManifestId = (int)MT.DocuSignEnvelope,
                       ProducedBy = "Query_DocuSign_v1"
                   }), AvailabilityType.RunTime);

                crateStorage.Add(availableRunTimeCrates);
            }

            return Task.FromResult(curActivityDO);
        }

        private Crate CreateControlsCrate()
        {
            var infobox = new TextBlock()
            {
                Value = "This activity doesn't require configuration"
            };

            return PackControlsCrate(infobox);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            return ConfigurationRequestType.Initial;
        }
    }
}