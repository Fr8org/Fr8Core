using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.States;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class Show_Report_Onscreen_v1 : BaseTerminalActivity
    {
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


        private async Task<Fr8Data.Crates.Crate> FindTables(ActivityDO curActivityDO)
        {
            var fields = new List<FieldDTO>();

            foreach (var table in (await GetCratesByDirection<FieldDescriptionsCM>(curActivityDO, CrateDirection.Upstream))
                                             .Select(x => x.Content)
                                             .SelectMany(x => x.Fields)
                                             .Where(x => x.Availability == AvailabilityType.RunTime && x.Value == "Table"))
            {
                fields.Add(new FieldDTO(table.Key, table.Key));    
            }

            return Fr8Data.Crates.Crate.FromContent("Upstream Crate Label List", new FieldDescriptionsCM(fields));
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Add(PackControls(new ActivityUi()));
                crateStorage.Add(await FindTables(curActivityDO));
            }

            return curActivityDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.ReplaceByLabel(await FindTables(curActivityDO));
            }

            return curActivityDO;
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);

            var configurationControls = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            if (configurationControls == null)
            {
                return Error(payload, "Action was not configured correctly");
            }

            var actionUi = new ActivityUi();

            actionUi.ClonePropertiesFrom(configurationControls);

            if (!string.IsNullOrWhiteSpace(actionUi.ReportSelector.SelectedLabel))
            {
                using (var crateStorage = CrateManager.GetUpdatableStorage(payload))
                {
                    var reportTable = crateStorage.CratesOfType<StandardPayloadDataCM>().FirstOrDefault(x => x.Label == actionUi.ReportSelector.SelectedLabel);

                    if (reportTable != null)
                    {
                        crateStorage.Add(Fr8Data.Crates.Crate.FromContent("Sql Query Result", new StandardPayloadDataCM
                        {
                            PayloadObjects = reportTable.Content.PayloadObjects
                        }));
                    }
                }
            }

            return ExecuteClientActivity(payload, "ShowTableReport");
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}