using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.Manifests;
using Data.States;
using Newtonsoft.Json;
using StructureMap;
using TerminalBase.BaseClasses;
using terminalSalesforce.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using Data.Constants;
using ServiceStack;

namespace terminalSalesforce.Actions
{
    public class Get_Data_v1 : EnhancedTerminalActivity<Get_Data_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList SalesforceObjectSelector { get; set; }

            public QueryBuilder SalesforceObjectFilter { get; set; }

            public ActivityUi()
            {
                SalesforceObjectSelector = new DropDownList
                {
                    Name = nameof(SalesforceObjectSelector),
                    Label = "Get Which Object?",
                    Required = true,
                    Events = new List<ControlEvent> {  ControlEvent.RequestConfig }
                };
                SalesforceObjectFilter = new QueryBuilder
                {
                    Name = nameof(SalesforceObjectFilter),
                    Label = "Meeting Which Conditions?",
                    Required = true,
                    Source = new FieldSourceDTO
                    {
                        Label = QueryFilterCrateLabel,
                        ManifestType = CrateManifestTypes.StandardQueryFields
                    }
                };
                Controls.Add(SalesforceObjectSelector);
                Controls.Add(SalesforceObjectFilter);
            }
        }
        //NOTE: this label must be the same as the one expected in QueryBuilder.ts
        public const string QueryFilterCrateLabel = "Queryable Criteria";

        public const string RuntimeDataCrateLabel = "Table from Salesforce Get Data";

        public const string SalesforceObjectFieldsCrateLabel = "Salesforce Object Fields";

        private readonly ISalesforceManager _salesforceManager;

        public Get_Data_v1() : base(true)
        {
            _salesforceManager = ObjectFactory.GetInstance<ISalesforceManager>();
            ActivityName = "Get Data from Salesforce";
        }

        protected override Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            ConfigurationControls.SalesforceObjectSelector.ListItems = _salesforceManager.GetSalesforceObjectTypes()
                                                                                .Select(x => new ListItem() { Key = x.Key, Value = x.Key })
                                                                                .ToList();
            runtimeCrateManager.MarkAvailableAtRuntime<StandardTableDataCM>(RuntimeDataCrateLabel);
            return Task.FromResult(true);
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            //If Salesforce object is empty then we should clear filters as they are no longer applicable
            var selectedObject = ConfigurationControls.SalesforceObjectSelector.selectedKey;
            if (string.IsNullOrEmpty(selectedObject))
            {
                CurrentActivityStorage.RemoveByLabel(QueryFilterCrateLabel);
                CurrentActivityStorage.RemoveByLabel(SalesforceObjectFieldsCrateLabel);
                this[nameof(ActivityUi.SalesforceObjectSelector)] = selectedObject;
                return;
            }
            //If the same object is selected we shouldn't do anything
            if (selectedObject == this[nameof(ActivityUi.SalesforceObjectSelector)])
            {
                return;
            }
            //Prepare new query filters from selected object properties
            var selectedObjectProperties = await _salesforceManager.GetProperties(selectedObject.ToEnum<SalesforceObjectType>(), AuthorizationToken);
            var queryFilterCrate = Crate<TypedFieldsCM>.FromContent(
                QueryFilterCrateLabel,
                new TypedFieldsCM(selectedObjectProperties.OrderBy(x => x.Key)
                                                                  .Select(x => new TypedFieldDTO(x.Key, x.Value, FieldType.String, new TextBox { Name = x.Key }))),
                AvailabilityType.Configuration);
            CurrentActivityStorage.ReplaceByLabel(queryFilterCrate);

            var objectPropertiesCrate = Crate<FieldDescriptionsCM>.FromContent(
                SalesforceObjectFieldsCrateLabel,
                new FieldDescriptionsCM(selectedObjectProperties),            
                AvailabilityType.RunTime);
            CurrentActivityStorage.ReplaceByLabel(objectPropertiesCrate);
            this[nameof(ActivityUi.SalesforceObjectSelector)] = selectedObject;
            //Publish information for downstream activities
            runtimeCrateManager.MarkAvailableAtRuntime<StandardTableDataCM>(RuntimeDataCrateLabel);
        }

        protected override async Task RunCurrentActivity()
        {
            var salesforceObject = ConfigurationControls.SalesforceObjectSelector.selectedKey;
            if (string.IsNullOrEmpty(salesforceObject))
            {
                throw new ActivityExecutionException("No Salesforce object is selected", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            var salesforceObjectFields = CurrentActivityStorage
                                            .FirstCrate<TypedFieldsCM>(x => x.Label == QueryFilterCrateLabel)
                                            .Content
                                            .Fields
                                            .Select(x => x.Name);
            var filterValue = ConfigurationControls.SalesforceObjectFilter.Value;
            var filterDataDTO = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(filterValue);
            //If without filter, just get all selected objects
            //else prepare SOQL query to filter the objects based on the filter conditions
            var parsedCondition = string.Empty;
            if (filterDataDTO.Count > 0)
            {
                parsedCondition = ParseConditionToText(filterDataDTO);
            }

            var resultObjects = await _salesforceManager.Query(salesforceObject.ToEnum<SalesforceObjectType>(), salesforceObjectFields, parsedCondition, AuthorizationToken);
            CurrentPayloadStorage.Add(Crate<StandardTableDataCM>.FromContent(RuntimeDataCrateLabel, resultObjects, AvailabilityType.RunTime));
        }
    }
}