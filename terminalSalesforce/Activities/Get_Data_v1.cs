using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Helpers;
using Newtonsoft.Json;
using ServiceStack;
using terminalSalesforce.Infrastructure;
using System;

namespace terminalSalesforce.Actions
{
    public class Get_Data_v1 : BaseSalesforceTerminalActivity<Get_Data_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("d8cf2810-87b9-43e7-a69b-a344823fd092"),
            Version = "1",
            Name = "Get_Data",
            Label = "Get Data from Salesforce",
            NeedsAuthentication = true,
            MinPaneWidth = 550,
            Tags = string.Join(",", Tags.TableDataGenerator, Tags.Getter),
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Receive,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList SalesforceObjectSelector { get; set; }

            public QueryBuilder SalesforceObjectFilter { get; set; }

            public ActivityUi()
            {
                SalesforceObjectSelector = new DropDownList
                {
                    Name = nameof(SalesforceObjectSelector),
                    Label = "Get all objects of type:",
                    Required = true,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                SalesforceObjectFilter = new QueryBuilder
                {
                    Name = nameof(SalesforceObjectFilter),
                    Label = "That meet the following conditions:",
                    Required = true,
                    Source = new FieldSourceDTO
                    {
                        Label = QueryFilterCrateLabel,
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    }
                };
                Controls.Add(SalesforceObjectSelector);
                Controls.Add(SalesforceObjectFilter);
            }
        }
        //NOTE: this label must be the same as the one expected in QueryBuilder.ts
        public const string QueryFilterCrateLabel = "Queryable Criteria";

        public const string RuntimeDataCrateLabel = "Table from Salesforce Get Data";

        public const string PayloadDataCrateLabel = "Payload from Salesforce Get Data";

        public const string CountObjectsFieldLabel = "Count of Objects";

        private const string ExternalObjectHandlesLabel = "External Object Handles";

        private readonly ISalesforceManager _salesforceManager;

        public Get_Data_v1(ICrateManager crateManager, ISalesforceManager salesforceManager)
            : base(crateManager)
        {
            _salesforceManager = salesforceManager;
        }

        public override Task Initialize()
        {
            ActivityUI.SalesforceObjectSelector.ListItems = _salesforceManager
                .GetSalesforceObjectTypes()
                .Select(x => new ListItem() { Key = x.Name, Value = x.Name })
                .ToList();
            CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>(RuntimeDataCrateLabel, true);

            return Task.FromResult(true);
        }

        public override async Task FollowUp()
        {
            //If Salesforce object is empty then we should clear filters as they are no longer applicable
            var selectedObject = ActivityUI.SalesforceObjectSelector.selectedKey;
            if (string.IsNullOrEmpty(selectedObject))
            {
                Storage.RemoveByLabel(QueryFilterCrateLabel);
                this[nameof(ActivityUi.SalesforceObjectSelector)] = selectedObject;
                return;
            }
            //If the same object is selected we shouldn't do anything
            if (selectedObject == this[nameof(ActivityUi.SalesforceObjectSelector)])
            {
                return;
            }
            //Prepare new query filters from selected object properties
            var selectedObjectProperties = await _salesforceManager
                .GetProperties(selectedObject.ToEnum<SalesforceObjectType>(), AuthorizationToken);
            var queryFilterCrate = Crate<FieldDescriptionsCM>.FromContent(
                QueryFilterCrateLabel,
                new FieldDescriptionsCM(selectedObjectProperties));
            Storage.ReplaceByLabel(queryFilterCrate);

            this[nameof(ActivityUi.SalesforceObjectSelector)] = selectedObject;
            //Publish information for downstream activities
            CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>(RuntimeDataCrateLabel, true)
                          .AddFields(selectedObjectProperties).AddField(CountObjectsFieldLabel);

            // Update ExternalObjectHandle crate.
            var externalObjectHandle = new ExternalObjectHandleDTO()
            {
                Name = selectedObject,
                Description = $"Data from Salesforce '{selectedObject}' object",
                DirectUrl = null,
                ManifestType = ManifestDiscovery.Default.GetManifestType<StandardTableDataCM>().Type
            };

            var externalObjectHandleCrate = Crate.FromContent(
                ExternalObjectHandlesLabel,
                new ExternalObjectHandlesCM(externalObjectHandle)
            );

            Storage.ReplaceByLabel(externalObjectHandleCrate);
        }

        public override async Task Run()
        {
            var salesforceObject = ActivityUI.SalesforceObjectSelector.selectedKey;
            if (string.IsNullOrEmpty(salesforceObject))
            {
                throw new ActivityExecutionException(
                    "No Salesforce object is selected",
                    ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            var salesforceObjectFields = Storage
                .FirstCrate<FieldDescriptionsCM>(x => x.Label == QueryFilterCrateLabel)
                .Content
                .Fields;

            var filterValue = ActivityUI.SalesforceObjectFilter.Value;
            var filterDataDTO = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(filterValue);
            //If without filter, just get all selected objects
            //else prepare SOQL query to filter the objects based on the filter conditions
            var parsedCondition = string.Empty;
            if (filterDataDTO.Count > 0)
            {
                parsedCondition = FilterConditionHelper.ParseConditionToText(filterDataDTO);
            }

            var resultObjects = await _salesforceManager
                .Query(
                    salesforceObject.ToEnum<SalesforceObjectType>(),
                    salesforceObjectFields,
                    parsedCondition,
                    AuthorizationToken
                );

            Payload.Add(
                Crate<StandardTableDataCM>
                    .FromContent(
                        RuntimeDataCrateLabel,
                        resultObjects
                    )
                );
        }
    }
}