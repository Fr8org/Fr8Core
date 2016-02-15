using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class Show_Report_Onscreen_v1 : BaseTerminalActivity
    {
        public class ActionUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public UpstreamDataChooser ReportSelector { get; set; }

            public ActionUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add((ReportSelector = new UpstreamDataChooser
                {
                    Name = "ReportSelector",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Label = "Display which table?"
                }));

                Controls.Add(new RunRouteButton());
            }
        }


        private async Task<Data.Crates.Crate> FindTables(ActivityDO curActivityDO)
        {
            var fields = new List<FieldDTO>();

            foreach (var table in (await GetCratesByDirection<StandardDesignTimeFieldsCM>(curActivityDO, CrateDirection.Upstream))
                                             .Select(x => x.Content)
                                             .SelectMany(x => x.Fields)
                                             .Where(x => x.Availability == AvailabilityType.RunTime && x.Value == "Table"))
            {
                fields.Add(new FieldDTO(table.Key, table.Key));    
            }

            return Data.Crates.Crate.FromContent("Upstream Crate Label List", new StandardDesignTimeFieldsCM(fields));
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = Crate.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Add(PackControls(new ActionUi()));
                crateStorage.Add(await FindTables(curActivityDO));
            }

            return curActivityDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = Crate.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.ReplaceByLabel(await FindTables(curActivityDO));
            }

            return curActivityDO;
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);

            var configurationControls = Crate.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            if (configurationControls == null)
            {
                return Error(payload, "Action was not configured correctly");
            }

            var actionUi = new ActionUi();

            actionUi.ClonePropertiesFrom(configurationControls);

            if (!string.IsNullOrWhiteSpace(actionUi.ReportSelector.SelectedLabel))
            {
                using (var crateStorage = Crate.GetUpdatableStorage(payload))
                {
                    var reportTable = crateStorage.CratesOfType<StandardPayloadDataCM>().FirstOrDefault(x => x.Label == actionUi.ReportSelector.SelectedLabel);

                    if (reportTable != null)
                    {
                        crateStorage.Add(Data.Crates.Crate.FromContent("Sql Query Result", new StandardPayloadDataCM
                        {
                            PayloadObjects = reportTable.Content.PayloadObjects
                        }));
                    }
                }
            }

            return ExecuteClientAction(payload, "ShowTableReport");
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (Crate.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}