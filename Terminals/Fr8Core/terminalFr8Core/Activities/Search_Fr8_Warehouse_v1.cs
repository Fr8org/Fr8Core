using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Models;
using Fr8.TerminalBase.Services;
using Hub.Services.MT;
using Newtonsoft.Json;
using StructureMap;

namespace terminalFr8Core.Activities
{
    /// <summary>
    ///  Not in service, but may provide useful ideas and insights
    /// </summary>
    public class Search_Fr8_Warehouse_v1 : ExplicitTerminalActivity
    {
        private readonly IContainer _container;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("33f353a1-65cc-4065-9517-71ddc0a7f4e2"),
            Name = "Search_Fr8_Warehouse",
            Label = "Search Fr8 Warehouse",
            Version = "1",
            NeedsAuthentication = false,
            MinPaneWidth = 400,
            Tags = Tags.HideChildren,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Solution,
                TerminalData.ActivityCategoryDTO
            }
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

        public Search_Fr8_Warehouse_v1(ICrateManager crateManager, IContainer container)
            : base(crateManager)
        {
            _container = container;
        }

        public override Task RunChildActivities()
        {
            var queryMTResult = Payload
                .CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "Found MT Objects")
                .FirstOrDefault();
            Payload.Add(Crate.FromContent("Sql Query Result", queryMTResult));
            RequestClientActivityExecution("ShowTableReport");
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
        protected override Task<DocumentationResponseDTO> GetDocumentation(string curDocumentation)
        {
            if (curDocumentation.Contains("MainPage"))
            {
                var curSolutionPage = new DocumentationResponseDTO(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);
            }
            return
                Task.FromResult(
                    new DocumentationResponseDTO("Unknown displayMechanism: we currently support MainPage cases"));
        }

        protected async Task GenerateSolutionActivities(string fr8ObjectID)
        {
            var queryFr8WarehouseAT = await HubCommunicator.GetActivityTemplate("terminalFr8Core", "Query_Fr8_Warehouse");
            var queryFr8WarehouseAction = await HubCommunicator.AddAndConfigureChildActivity(ActivityPayload, queryFr8WarehouseAT);

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

            queryFr8WarehouseAction = await HubCommunicator.ConfigureChildActivity(
                ActivityContext.ActivityPayload,
                queryFr8WarehouseAction
            );
        }

        private void LoadAvailableFr8ObjectNames(string fr8ObjectID)
        {
            using (var uow = _container.GetInstance<IUnitOfWork>())
            {
                var designTimeQueryFields = MTTypesHelper.GetFieldsByTypeId(uow, Guid.Parse(fr8ObjectID));
                var criteria = Storage.FirstOrDefault(d => d.Label == "Queryable Criteria");
                if (criteria != null)
                {
                    Storage.Remove(criteria);
                }
                Storage.Add(Crate.FromContent("Queryable Criteria", new FieldDescriptionsCM(designTimeQueryFields)));
            }
        }

        private void UpdateOperationCrate(string errorMessage = null)
        {
            Storage.RemoveByManifestId((int)MT.OperationalStatus);
            var operationalStatus = new OperationalStateCM
            {
                CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.ExecuteClientActivity),
            };

            operationalStatus.CurrentActivityResponse.Body = "RunImmediately";

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

            Storage.Remove<ValidationResultsCM>();
            
            var validationManager = new ValidationManager(Payload);

            if (string.IsNullOrWhiteSpace(fr8Object))
            {
                validationManager.SetError("Please select the Fr8 Object", fr8ObjectDropDown);
                Storage.Add(Crate.FromContent("Validation Result", validationManager.ValidationResults));
                return false;
            }

            return true;
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


        // create the dropdown design time fields.
        private List<KeyValueDTO> GetFr8WarehouseTypes(AuthorizationToken oAuthToken)
        {
            using (var unitWork = _container.GetInstance<IUnitOfWork>())
            {
                var warehouseTypes = new List<KeyValueDTO>();

                foreach (var mtTypeReference in unitWork.MultiTenantObjectRepository.ListTypeReferences())
                {
                    warehouseTypes.Add(new KeyValueDTO
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
            AddControls(new ActionUi().Controls);
            var designTimefieldLists = GetFr8WarehouseTypes(AuthorizationToken);

            Storage.Add("Queryable Objects", new KeyValueListCM(designTimefieldLists));

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
                Logger.GetLogger().Error($"Error while configuring the search Fr8 Warehouse action {e}");
                throw;
            }
        }
    }
}