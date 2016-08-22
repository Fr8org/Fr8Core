using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using TerminalBase.BaseClasses;

namespace terminalFr8Core.Actions
{
    public class Show_Report_Onscreen_v2
        : EnhancedTerminalActivity<Show_Report_Onscreen_v2.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public CrateChooser ReportSelector { get; set; }

            public ActivityUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                ReportSelector = new CrateChooser
                {
                    Label = "Show report using data from",
                    Name = "Available_Crates",
                    SingleManifestOnly = true,
                    RequestUpstream = true
                };
                Controls.Add(ReportSelector);

                Controls.Add(new RunPlanButton());
            }
        }

        public Show_Report_Onscreen_v2() : base(false)
        {
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            await Task.Yield();
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            await Task.Yield();
        }

        protected override async Task RunCurrentActivity()
        {
            var crate = FindCrateToProcess(CurrentPayloadStorage);
            var payloadObjects = new List<PayloadObjectDTO>();

            if (crate.IsOfType<StandardTableDataCM>())
            {
                var table = crate.Get<StandardTableDataCM>();
                if (table.FirstRowHeaders)
                {
                    payloadObjects.AddRange(
                        table.DataRows.Select(x => new PayloadObjectDTO(x.Row.Select(y => y.Cell)))
                    );
                }
            }

            CurrentPayloadStorage.Add(
                Fr8Data.Crates.Crate.FromContent(
                    "Sql Query Result",
                    new StandardPayloadDataCM
                    {
                        PayloadObjects = payloadObjects
                    }
                )
            );

            OperationalState.CurrentActivityResponse =
                ActivityResponseDTO.Create(ActivityResponse.ExecuteClientActivity);
            OperationalState.CurrentClientActivityName = "ShowTableReport";

            await Task.Yield();
        }

        private Crate FindCrateToProcess(ICrateStorage payloadStorage)
        {
            var selectedCrateDescription = ConfigurationControls
                .ReportSelector
                .CrateDescriptions.Single(c => c.Selected);

            return payloadStorage
                .FirstOrDefault(c => c.ManifestType.Type == selectedCrateDescription.ManifestType
                    && c.Label == selectedCrateDescription.Label);
        }

        /*
        private async Task<Data.Crates.Crate> FindTables(ActivityDO curActivityDO)
        {
            var fields = new List<FieldDTO>();

            foreach (var table in (await GetCratesByDirection<FieldDescriptionsCM>(curActivityDO, CrateDirection.Upstream))
                                             .Select(x => x.Content)
                                             .SelectMany(x => x.Fields)
                                             .Where(x => x.Availability == AvailabilityType.RunTime && x.Value == "Table"))
            {
                fields.Add(new FieldDTO(table.Key, table.Key));    
            }

            return Data.Crates.Crate.FromContent("Upstream Crate Label List", new FieldDescriptionsCM(fields));
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
                        crateStorage.Add(Data.Crates.Crate.FromContent("Sql Query Result", new StandardPayloadDataCM
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
        */
    }
}