using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Newtonsoft.Json;
using StructureMap;
using terminalFr8Core.Services.MT;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalBase.Models;
using Utilities.Logging;

namespace terminalFr8Core.Activities
{
    public class SearchFr8Warehouse_v1 : BaseTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "SearchFr8Warehouse",
            Label = "Search Fr8 Warehouse",
            Version = "1",
            Category = ActivityCategory.Solution,
            NeedsAuthentication = false,
            MinPaneWidth = 400,
            Tags = Tags.HideChildren,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

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

        public SearchFr8Warehouse_v1() : base(false)
        {
        }

        public override Task RunChildActivities()
        {
            var queryMTResult = Payload
                .CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "Found MT Objects")
                .FirstOrDefault();
            Payload.Add(Crate.FromContent("Sql Query Result", queryMTResult));
            ExecuteClientActivity("ShowTableReport");
            return Task.FromResult(0);
        }

        /// <summary>
        /// This method provides documentation in two forms:
        /// SolutionPageDTO for general information and 
        /// ActivityResponseDTO for specific Help on minicon
        /// </summary>
        /// <param name="activityPayload"></param>
        /// <param name="curDocumentation"></param>
        /// <returns></returns>
        public dynamic Documentation(ActivityPayload activityPayload, string curDocumentation)
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

        protected async Task GenerateSolutionActivities(string fr8ObjectID)
        {
            var queryFr8WarehouseAT = await GetActivityTemplate("terminalFr8Core", "QueryFr8Warehouse");
            var queryFr8WarehouseAction = await AddAndConfigureChildActivity(
                ActivityId, queryFr8WarehouseAT
            );

            var crateStorage = queryFr8WarehouseAction.CrateStorage;
            // We insteady of using getConfiguration control used the same GetConfiguration control required actionDO
            var queryFr8configurationControls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            var radioButtonGroup = queryFr8configurationControls.FindByName<RadioButtonGroup>("QueryPicker");
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
                var queryBuilderControl = GetControl<QueryBuilder>("QueryBuilder");
                var criteria = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(queryBuilderControl.Value);
                FilterDataDTO filterPaneDTO = new FilterDataDTO();
                filterPaneDTO.Conditions = criteria;
                filterPaneDTO.ExecutionType = FilterExecutionType.WithFilter;
                upstreamCrateChooser1.Value = JsonConvert.SerializeObject(filterPaneDTO);
                upstreamCrateChooser1.Selected = true;
            }

            queryFr8WarehouseAction = await ConfigureChildActivity(
                ActivityContext.ActivityPayload,
                queryFr8WarehouseAction
            );
        }

        private void LoadAvailableFr8ObjectNames(string fr8ObjectID)
        {
            var designTimeQueryFields = MTTypesHelper.GetFieldsByTypeId(Guid.Parse(fr8ObjectID));
            var criteria = Storage.FirstOrDefault(d => d.Label == "Queryable Criteria");
            if (criteria != null)
            {
                Storage.Remove(criteria);
            }
            Storage.Add(Crate.FromContent("Queryable Criteria",new FieldDescriptionsCM(designTimeQueryFields)));
        }

        private void UpdateOperationCrate(string errorMessage = null)
        {
            Storage.RemoveByManifestId((int)MT.OperationalStatus);
            var operationalStatus = new OperationalStateCM
            {
                CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.ExecuteClientActivity),
                CurrentClientActivityName = "RunImmediately"
            };
            var operationsCrate = Crate.FromContent("Operational Status", operationalStatus);
            Storage.Add(operationsCrate);
        }

        private void UpdateQueryCrate(string fr8ObjectID)
        {
            Storage.Remove<StandardQueryCM>();
            var queryCrate = ExtractQueryCrate(fr8ObjectID);
            Storage.Add(queryCrate);
        }

        private bool ValidateSolutionInputs(string fr8Object)
        {
            var fr8ObjectDropDown = GetControl<DropDownList>("Select Fr8 Warehouse Object");
            var validationResult = Storage.GetOrAdd(() => Crate.FromContent("Validation Result", new ValidationResultsCM()));
            var validationManager = new ValidationManager(validationResult);

            if (String.IsNullOrWhiteSpace(fr8Object))
            {
                validationManager.SetError("Please select the Fr8 Object", fr8ObjectDropDown);
                return false;
            }
                
            return true;
        }

        private static ControlDefinitionDTO CreateTextBoxQueryControl(
            string key)
        {
            return new TextBox()
            {
                Name = "QueryField_" + key
            };
        }

        private Crate<StandardQueryCM> ExtractQueryCrate(string mtObject)
        {
            var configurationControls = Storage
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
            yield return Crate.FromContent("Fr8 Search Report", new FieldDescriptionsCM(new FieldDTO
            {
                Key = "Fr8 Search Report",
                Value = "Table",
                Availability = AvailabilityType.RunTime
            }));
        }

        // create the dropdown design time fields.
        private List<FieldDTO> GetFr8WarehouseTypes(AuthorizationToken oAuthToken)
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

        public override Task Run()
        {
            Success();
            return Task.FromResult(0);
        }

        public override Task Initialize()
        {
            Storage.Add(PackControlsCrate(new ActionUi().Controls.ToArray()));
            var designTimefieldLists = GetFr8WarehouseTypes(AuthorizationToken);
            var availableMtObjects = CrateManager.CreateDesignTimeFieldsCrate("Queryable Objects", designTimefieldLists.ToArray());
            Storage.Add(availableMtObjects);
            Storage.AddRange(PackDesignTimeData());
            return Task.FromResult(0);
        }

        public override async Task FollowUp()
        {
            try
            {
                var fr8ObjectDropDown = GetControl<DropDownList>("Select Fr8 Warehouse Object");
                var fr8ObjectID = fr8ObjectDropDown.Value;
                var continueButton = GetControl<Button>("Continue");
                if (ButtonIsClicked(continueButton))
                {
                    if (!ValidateSolutionInputs(fr8ObjectID))
                    {
                        UpdateQueryCrate(fr8ObjectID);
                        return;
                    }

                    UpdateQueryCrate(fr8ObjectID);

                    ActivityContext.ActivityPayload.ChildrenActivities.Clear();
                    await GenerateSolutionActivities(fr8ObjectID);
                    UpdateOperationCrate();
                }
                else
                {
                    LoadAvailableFr8ObjectNames(fr8ObjectID);
                }
            }
            catch (Exception e)
            {
                // This message will get display in Terminal Activity Response.
                //Logger.GetLogger().Error("Error while configuring the search Fr8 Warehouse action" + e.Message, e);
                Logger.LogError($"Error while configuring the search Fr8 Warehouse action {e}");
                throw;
            }
        }
    }
}