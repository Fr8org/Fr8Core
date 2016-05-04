using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalBase.Services.MT;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Utilities.Logging;

namespace terminalFr8Core.Actions
{
    public class SearchFr8Warehouse_v1 : BaseTerminalActivity
    {
        private const string QueryCrateLabel = "Fr8 Search Query";
        private const string SolutionName = "Search Fr8 Warehouse";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "terminalFr8Core";
        private const string SolutionBody = @"<p>The Search Fr8 Warehouse solution allows you to search the Fr8 Warehouse 
                                            for information we're storing for you. This might be event data about your cloud services that we track on your 
                                            behalf. Or it might be files or data that your plans have stored.</p>";

        // Here in this action we have query builder control to build queries against MT database.
        // Note We are ignoring the generic type searching and fetching  FR-2317

        public class ActionUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public QueryBuilder QueryBuilder { get; set; }

            public ActionUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(new TextArea
                {
                    IsReadOnly = true,
                    Label = "",
                    Value = "<p>Search for Fr8 Warehouse where the following are true:</p>"
                });

                Controls.Add(new DropDownList()
                {
                    Name = "Select Fr8 Warehouse Object",
                    Required = true,
                    Label = "Select Fr8 Warehouse Object",
                    Source = new FieldSourceDTO
                    {
                        Label = "Queryable Objects",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    },
                    Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
                });

                Controls.Add((QueryBuilder = new QueryBuilder
                {
                    Name = "QueryBuilder",
                    Value = null,
                    Source = new FieldSourceDTO
                    {
                        Label = "Queryable Criteria",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    }
                }));

                Controls.Add(new Button()
                {
                    Label = "Continue",
                    Name = "Continue",
                    Events = new List<ControlEvent>()
                    {
                        new ControlEvent("onClick", "requestConfig")
                    }
                });
            }
        }

        public SearchFr8Warehouse_v1()
        {
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);
            return Success(payload);
        }

        public override async Task<PayloadDTO> ExecuteChildActivities(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);

            var configurationControls = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            if (configurationControls == null)
            {
                return Error(payload, "Action was not configured correctly");
            }

            // Merge data from QueryMT action.
            var payloadCrateStorage = CrateManager.FromDto(payload.CrateStorage);
            var queryMTResult = payloadCrateStorage
                .CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "Found MT Objects")
                .FirstOrDefault();

            using (var crateStorage = CrateManager.GetUpdatableStorage(payload))
            {
                crateStorage.Add(Fr8Data.Crates.Crate.FromContent("Sql Query Result", queryMTResult));
            }

            return ExecuteClientActivity(payload, "ShowTableReport");
        }


        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Add(PackControls(new ActionUi()));
                var designTimefieldLists = GetFr8WarehouseTypes(authTokenDO);
                var availableMtObjects = CrateManager.CreateDesignTimeFieldsCrate("Queryable Objects", designTimefieldLists.ToArray());
                crateStorage.Add(availableMtObjects);
                crateStorage.AddRange(PackDesignTimeData());
            }
            return Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            try
            {
                var configurationcontrols = GetConfigurationControls(activityDO);
                var fr8ObjectDropDown = GetControl(configurationcontrols, "Select Fr8 Warehouse Object");
                var fr8ObjectID = fr8ObjectDropDown.Value;
                var continueButton = configurationcontrols.FindByName<Button>("Continue");

                if (ButtonIsClicked(continueButton))
                {
                    if (!ValidateSolutionInputs(fr8ObjectID))
                    {
                        AddRemoveCrateAndError(activityDO, fr8ObjectID, "Please select the Fr8 Object");
                        return activityDO;
                    }
                    else {
                        AddRemoveCrateAndError(activityDO, fr8ObjectID, "");
                    }

                    activityDO.ChildNodes.Clear();
                    await GenerateSolutionActivities(activityDO, fr8ObjectID);
                    UpdateOperationCrate(activityDO);
                }
                else {
                    LoadAvailableFr8ObjectNames(activityDO, fr8ObjectID);
                }
            }
            catch (Exception e)
            {
                // This message will get display in Terminal Activity Response.
                //Logger.GetLogger().Error("Error while configuring the search Fr8 Warehouse action" + e.Message, e);
                Logger.LogError($"Error while configuring the search Fr8 Warehouse action {e}");
                throw;
            }

            return activityDO;
        }

        /// <summary>
        /// This method provides documentation in two forms:
        /// SolutionPageDTO for general information and 
        /// ActivityResponseDTO for specific Help on minicon
        /// </summary>
        /// <param name="activityDO"></param>
        /// <param name="curDocumentation"></param>
        /// <returns></returns>
        public dynamic Documentation(ActivityDO activityDO, string curDocumentation)
        {
            if (curDocumentation.Contains("MainPage"))
            {
                var curSolutionPage = GetDefaultDocumentation(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);
            }
            return
                Task.FromResult(
                    GenerateErrorResponse("Unknown displayMechanism: we currently support MainPage cases"));
        }

        protected async Task<ActivityDO> GenerateSolutionActivities(ActivityDO activityDO, string fr8ObjectID)
        {
            var queryFr8WarehouseAT = await GetActivityTemplate("terminalFr8Core", "QueryFr8Warehouse");
            var queryFr8WarehouseAction = await AddAndConfigureChildActivity(
                activityDO, queryFr8WarehouseAT
            );

            using (var crateStorage = CrateManager.GetUpdatableStorage(queryFr8WarehouseAction))
            {
                // We insteady of using getConfiguration control used the same GetConfiguration control required actionDO
                var queryFr8configurationControls = crateStorage.
                    CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

                var radioButtonGroup = queryFr8configurationControls
                    .FindByName<RadioButtonGroup>("QueryPicker");

                DropDownList fr8ObjectDropDown = null;

                if (radioButtonGroup != null
                    && radioButtonGroup.Radios.Count > 0
                    && radioButtonGroup.Radios[0].Controls.Count > 0)
                {
                    fr8ObjectDropDown = radioButtonGroup.Radios[1].Controls[0] as DropDownList;
                    radioButtonGroup.Radios[1].Selected = true;
                    radioButtonGroup.Radios[0].Selected = false;
                }

                if (fr8ObjectDropDown != null)
                {
                    fr8ObjectDropDown.Selected = true;
                    fr8ObjectDropDown.Value = fr8ObjectID;
                    fr8ObjectDropDown.selectedKey = fr8ObjectID;

                    FilterPane upstreamCrateChooser1 = radioButtonGroup.Radios[1].Controls[1] as FilterPane;

                    var configurationControls = GetConfigurationControls(activityDO);
                    var queryBuilderControl = configurationControls.FindByName<QueryBuilder>("QueryBuilder");
                    var criteria = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(queryBuilderControl.Value);

                    FilterDataDTO filterPaneDTO = new FilterDataDTO();
                    filterPaneDTO.Conditions = criteria;
                    filterPaneDTO.ExecutionType = FilterExecutionType.WithFilter;
                    upstreamCrateChooser1.Value = JsonConvert.SerializeObject(filterPaneDTO);
                    upstreamCrateChooser1.Selected = true;
                }
            }

            queryFr8WarehouseAction = await ConfigureChildActivity(
                activityDO,
                queryFr8WarehouseAction
            );

            return activityDO;
        }

        private void LoadAvailableFr8ObjectNames(ActivityDO actvityDO, string fr8ObjectID)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(actvityDO))
            {
                var designTimeQueryFields = MTTypesHelper.GetFieldsByTypeId(Guid.Parse(fr8ObjectID));
                var criteria = crateStorage.FirstOrDefault(d => d.Label == "Queryable Criteria");
                if (criteria != null)
                {
                    crateStorage.Remove(criteria);
                }
                crateStorage.Add(
                    Fr8Data.Crates.Crate.FromContent(
                        "Queryable Criteria",
                        new FieldDescriptionsCM(designTimeQueryFields)
                    )
                );
            }
        }

        private void UpdateOperationCrate(ActivityDO activityDO, string errorMessage = null)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(activityDO))
            {
                crateStorage.RemoveByManifestId((int)MT.OperationalStatus);
                var operationalStatus = new OperationalStateCM();

                operationalStatus.CurrentActivityResponse =
                    ActivityResponseDTO.Create(ActivityResponse.ExecuteClientActivity);
                operationalStatus.CurrentClientActivityName = "RunImmediately";
                var operationsCrate = Fr8Data.Crates.Crate.FromContent("Operational Status", operationalStatus);
                crateStorage.Add(operationsCrate);
            }
        }

        private void AddRemoveCrateAndError(ActivityDO activityDO,string fr8ObjectID,string errorMessage)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(activityDO))
            {
                crateStorage.Remove<StandardQueryCM>();
                var queryCrate = ExtractQueryCrate(crateStorage, fr8ObjectID);
                crateStorage.Add(queryCrate);

                var configurationcontrols = crateStorage.
                  CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
                var fr8ObjectDropDown = GetControl(configurationcontrols, "Select Fr8 Warehouse Object");
                fr8ObjectDropDown.ErrorMessage = errorMessage;
            }
        }

        private bool ValidateSolutionInputs(string fr8Object)
        {
            if (String.IsNullOrWhiteSpace(fr8Object))
            {
                return false;
            }
            return true;
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }
            return ConfigurationRequestType.Followup;
        }

        private static ControlDefinitionDTO CreateTextBoxQueryControl(
            string key)
        {
            return new TextBox()
            {
                Name = "QueryField_" + key
            };
        }

        private Crate<StandardQueryCM> ExtractQueryCrate(ICrateStorage storage, string mtObject)
        {
            var configurationControls = storage
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .SingleOrDefault();

            if (configurationControls == null)
            {
                throw new ApplicationException("Action was not configured correctly");
            }

            var actionUi = new ActionUi();
            actionUi.ClonePropertiesFrom(configurationControls);

            var criteria = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(
                actionUi.QueryBuilder.Value
            );

            var queryCM = new StandardQueryCM(
                new QueryDTO()
                {
                    Name = mtObject,
                    Criteria = criteria
                }
            );

            return Crate<StandardQueryCM>.FromContent(QueryCrateLabel, queryCM);
        }

        private IEnumerable<Crate> PackDesignTimeData()
        {
            yield return Fr8Data.Crates.Crate.FromContent("Fr8 Search Report", new FieldDescriptionsCM(new FieldDTO
            {
                Key = "Fr8 Search Report",
                Value = "Table",
                Availability = AvailabilityType.RunTime
            }));
        }

        // create the dropdown design time fields.
        private List<FieldDTO> GetFr8WarehouseTypes(AuthorizationTokenDO oAuthToken)
        {
            using (var unitWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var warehouseTypes = new List<FieldDTO>();

                foreach (var mtTypeReference in unitWork.MultiTenantObjectRepository.ListTypeReferences())
                {
                    warehouseTypes.Add(new FieldDTO
                    {
                        Key = mtTypeReference.Alias,
                        Value = mtTypeReference.Id.ToString("N")
                    });
                }
                
                return warehouseTypes;
            }
        }

        private bool ButtonIsClicked(Button button)
        {
            if (button != null && button.Clicked)
            {
                return true;
            }
            return false;
        }

    }
}