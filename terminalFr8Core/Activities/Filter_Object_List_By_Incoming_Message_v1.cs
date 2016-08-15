using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Helpers;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Services;

namespace terminalFr8Core.Activities
{
    public class Filter_Object_List_By_Incoming_Message_v1 : TerminalActivity<Filter_Object_List_By_Incoming_Message_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("36470147-05e3-4f32-94ef-bf203b6c53af"),
            Name = "Filter_Object_List_By_Incoming_Message",
            Label = "Filter Object List by Incoming Message",
            Version = "1",
            NeedsAuthentication = false,
            MinPaneWidth = 400,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public UpstreamFieldChooser IncomingTextSelector { get; set; }

            public DropDownList DataSourceSelector { get; set; }

            public TextBox KeywordPropertiesSource { get; set; }

            public TextBlock KeywordPropertiesSourceDescription { get; set; }

            public ActivityUi() : this(new UiBuilder())
            {
            }

            public ActivityUi(UiBuilder uiBuilder)
            {
                DataSourceSelector = new DropDownList
                {
                    Name = nameof(DataSourceSelector),
                    Label = "Build the CheckList of objects",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                IncomingTextSelector = new UpstreamFieldChooser
                {
                    Name = nameof(IncomingTextSelector),
                    Label = "Compare CheckList against which incoming text",
                    Source = new FieldSourceDTO
                    {
                        AvailabilityType = AvailabilityType.RunTime,
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                        RequestUpstream = true
                    },
                };
                KeywordPropertiesSource = new TextBox
                {
                    Name = nameof(KeywordPropertiesSource),
                    Label = "Build an UpdateList containing CheckList objects where one of the below properties matches a word in the Incoming text"
                };
                KeywordPropertiesSourceDescription = new TextBlock
                {
                    Name = nameof(KeywordPropertiesSourceDescription),
                    Value = "Multiple values should be separated with commas"
                };
                Controls.Add(DataSourceSelector);
                Controls.Add(IncomingTextSelector);
                Controls.Add(KeywordPropertiesSource);
                Controls.Add(KeywordPropertiesSourceDescription);
            }
        }

        private static readonly string[] StopwordsList = new[] { "the", "company", "co", "inc", "incorporate", "a", "an" };

        private const string CacheCratedAt = "Cache Created At";

        private const string CacheDateFormat = "yyyyMMddHHmmss";

        private static readonly TimeSpan CacheExpirationTime = TimeSpan.FromHours(1.0);

        public Filter_Object_List_By_Incoming_Message_v1(ICrateManager crateManager)
            : base(crateManager)
        {
            //ActivityName = "Match Incoming Text and Build Object List";
        }

        public override async Task Initialize()
        {
            var activityTemplates = await HubCommunicator.GetActivityTemplates(Tags.TableDataGenerator);
            activityTemplates.Sort((x, y) => x.Name.CompareTo(y.Name));
            ActivityUI.DataSourceSelector.ListItems = activityTemplates
                                                                 .Select(x => new ListItem { Key = x.Label, Value = x.Id.ToString() })
                                                                 .ToList();
        }

        public override async Task FollowUp()
        {
            //Remove child activity if its not specified or add it if is not yet added
            if (string.IsNullOrEmpty(ActivityUI.DataSourceSelector.Value))
            {
                await HubCommunicator.DeleteExistingChildNodesFromActivity(ActivityId);
                ActivityContext.ActivityPayload.ChildrenActivities.Clear();
                PreviousSelectedDataSourceId = null;
                CachedData = null;
            }
            else if (string.IsNullOrEmpty(PreviousSelectedDataSourceId) || PreviousSelectedDataSourceId != ActivityUI.DataSourceSelector.Value)
            {
                var activityTemplate = await HubCommunicator.GetActivityTemplate(Guid.Parse(ActivityUI.DataSourceSelector.Value));
                await HubCommunicator.DeleteExistingChildNodesFromActivity(ActivityId);
                ActivityContext.ActivityPayload.ChildrenActivities.Clear();
                await HubCommunicator.AddAndConfigureChildActivity(ActivityContext.ActivityPayload, activityTemplate, order: 1);
                PreviousSelectedDataSourceId = ActivityUI.DataSourceSelector.Value;
                CachedData = null;
            }
        }

        public override async Task Run()
        {
            if (IsInitialRun)
            {
                ValidateRunPrerequisites();
                //If we don't want to use cached data or it it too old then we let child activity to generate new data
                if (CachedData == null || CachedDataIsOld)
                {
                    CachedData = null;
                    return;
                }
            }
            //Here we come after child activity is executed OR we want to use cached data and we have it
            var dataToFilter = GetData();
            if (dataToFilter == null)
            {
                throw new ActivityExecutionException($"Data source activity didn't generated {MT.StandardTableData.GetEnumDisplayName()} crate");
            }
            //If child activity did ran this time we need to remove the data its produced from payload (as it represents unfiltered data)
            Payload.Remove(dataToFilter);
            var dataProperties = GetDataProperties(dataToFilter.Content);
            var result = FilterData(IncomingText, dataToFilter, dataProperties);
            Payload.Add(result);
            IsInitialRun = true;
            //We don't want to run child activity again as we already used its data
            RequestSkipChildren();
        }

        public override Task RunChildActivities()
        {
            //We want to run our activity in filtering mode after child activity is completed
            RequestJumpToActivity(ActivityId);
            IsInitialRun = false;
            return Task.FromResult(0);
        }

        #region Implementation details

        private string PreviousSelectedDataSourceId
        {
            get { return this[nameof(ActivityUi.DataSourceSelector)]; }
            set { this[nameof(ActivityUi.DataSourceSelector)] = value; }
        }

        private Crate<StandardTableDataCM> FilterData(string message, Crate<StandardTableDataCM> data, HashSet<string> dataProperties)
        {
            //If there is nothing to filter we just return the original data from child activity
            if (string.IsNullOrWhiteSpace(message))
            {
                return data;
            }
            var sourceTable = data.Content;
            var result = new StandardTableDataCM { Table = new List<TableRowDTO>(), FirstRowHeaders = sourceTable.FirstRowHeaders };
            //Copy header row
            if (sourceTable.FirstRowHeaders && sourceTable.Table.Count > 0)
            {
                result.Table.Add(sourceTable.Table[0]);
            }
            foreach (var dataRow in sourceTable.DataRows)
            {
                var dataRowPropertyValues = GetPropertyValuesFromDataRow(dataRow, dataProperties);
                var propertyValueIsFound = false;
                foreach (var propertyValue in dataRowPropertyValues)
                {
                    if (message.Contains(propertyValue, StringComparison.InvariantCultureIgnoreCase))
                    {
                        propertyValueIsFound = true;
                        break;
                    }
                }
                if (propertyValueIsFound)
                {
                    result.Table.Add(dataRow);
                }
            }
            return Crate<StandardTableDataCM>.FromContent(data.Label, result);
        }

        private string[] GetPropertyValuesFromDataRow(TableRowDTO dataRow, HashSet<string> dataProperties)
        {
            var result = dataRow.Row
                                .Select(x => x.Cell)
                                .Where(x => dataProperties == null || dataProperties.Count == 0 || dataProperties.Contains(x.Key))
                                .Select(x => x.Value)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Select(RemoveStopwords);
            return result.ToArray();
        }

        private string RemoveStopwords(string value)
        {
            return value.RemoveStopwords(StopwordsList);
        }

        private Crate<StandardTableDataCM> GetData()
        {
            return CachedData ?? (CachedData = NewData);
        }

        private HashSet<string> GetDataProperties(StandardTableDataCM data)
        {
            if (!data.HasDataRows || data.Table[0].Row.Count == 0)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(SpecifiedDataProperties))
            {
                return null;
            }
            return new HashSet<string>(SpecifiedDataProperties.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()), StringComparer.InvariantCultureIgnoreCase);
        }

        private Crate<StandardTableDataCM> NewData
        {
            get
            {
                //TODO: we expect that activities that generate table data have to publish crate description with this data
                //If this assumption won't be accepted then we'll have to compare payload before and after child activity is executed to get the data generated by child activity
                var childActivityStorage = ActivityContext.ActivityPayload.ChildrenActivities.OrderBy(x => x.Ordering).ToList()[0].CrateStorage;
                var dataDescription = childActivityStorage.FirstCrateOrDefault<CrateDescriptionCM>()
                                                         ?.Content
                                                          .CrateDescriptions
                                                          .FirstOrDefault(x => x.ManifestId == (int)MT.StandardTableData);
                if (dataDescription == null)
                {
                    return null;
                }
                return Payload.FirstCrateOrDefault<StandardTableDataCM>(x => x.Label == dataDescription.Label);
            }
        }

        private Crate<StandardTableDataCM> CachedData
        {
            get
            {
                return Storage.FirstCrateOrDefault<StandardTableDataCM>();
            }
            set
            {
                if (value == null)
                {
                    Storage.Remove<StandardTableDataCM>();
                    this[CacheCratedAt] = null;
                }
                else
                {
                    Storage.Remove<StandardTableDataCM>();
                    Storage.Add(value);
                    this[CacheCratedAt] = DateTime.UtcNow.ToString(CacheDateFormat); 
                }
            }
        }

        private bool IsInitialRun
        {
            get { return Payload.All(x => x.Label != ActivityId.ToString()); }
            set
            {
                if (value)
                {
                    Payload.RemoveByLabel(ActivityId.ToString());
                }
                else
                {
                    Payload.ReplaceByLabel(Crate<StandardPayloadDataCM>.FromContent(ActivityId.ToString(), new StandardPayloadDataCM()));
                }
            }
        }

        private void ValidateRunPrerequisites()
        {
            if (string.IsNullOrEmpty(SelectedDataSourceActivityId))
            {
                throw new ActivityExecutionException("Data source is not specified", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            if (ActivityContext.ActivityPayload.ChildrenActivities.Count == 0)
            {
                throw new ActivityExecutionException("Data source activity is missing");
            }
            /*var datasourceActivity = ActivityContext.ActivityPayload.ChildrenActivities.OrderBy(x => x.Ordering).ToList()[0];
            if (datasourceActivity.ActivityTemplate.Id != Guid.Parse(SelectedDataSourceActivityId))
            {
                throw new ActivityExecutionException("Data source activity is other than specified in data source");
            }*/
        }
        //Wrappers for control properties
        private string IncomingText => ActivityUI.IncomingTextSelector.Value;

        private string SelectedDataSourceActivityId => ActivityUI.DataSourceSelector.Value;
        
        public bool CachedDataIsOld
        {
            get
            {
                DateTime cacheCratedAt;
                if (!DateTime.TryParseExact(this[CacheCratedAt], CacheDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out cacheCratedAt))
                {
                    return true;
                }
                var cacheAge = DateTime.UtcNow.Subtract(cacheCratedAt);
                //For now we use just constant value
                return cacheAge > CacheExpirationTime;
            }
        }

        private string SpecifiedDataProperties => ActivityUI.KeywordPropertiesSource.Value;
        
        #endregion

    }
}