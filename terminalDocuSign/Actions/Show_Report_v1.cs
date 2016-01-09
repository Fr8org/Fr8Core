using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Show_Report_v1 : BaseTerminalAction
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


        private async Task<Data.Crates.Crate> FindTables(ActionDO curActionDO)
        {
            var fields = new List<FieldDTO>();

            foreach (var table in (await GetCratesByDirection<StandardDesignTimeFieldsCM>(curActionDO, CrateDirection.Upstream))
                                             .Select(x => x.Content)
                                             .SelectMany(x => x.Fields)
                                             .Where(x => x.Availability == AvailabilityType.RunTime && x.Value == "Table"))
            {
                fields.Add(new FieldDTO(table.Key, table.Key));    
            }

            return Data.Crates.Crate.FromContent("Upstream Crate Label List", new StandardDesignTimeFieldsCM(fields));
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.Add(await FindTables(curActionDO));
            }

            return curActionDO;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.ReplaceByLabel(await FindTables(curActionDO));
            }

            return curActionDO;
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActionDO, containerId);

            var configurationControls = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            if (configurationControls == null)
            {
                return Error(payload, "Action was not configured correctly");
            }

            var actionUi = new ActionUi();

            actionUi.ClonePropertiesFrom(configurationControls);

            if (!string.IsNullOrWhiteSpace(actionUi.ReportSelector.SelectedLabel))
            {
                using (var updater = Crate.UpdateStorage(payload))
                {
                    var reportTable = updater.CrateStorage.CratesOfType<StandardPayloadDataCM>().FirstOrDefault(x => x.Label == actionUi.ReportSelector.SelectedLabel);

                    if (reportTable != null)
                    {
                        updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Sql Query Result", new StandardPayloadDataCM
                        {
                            PayloadObjects = reportTable.Content.PayloadObjects
                        }));
                    }
                }
            }

            return payload;
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}