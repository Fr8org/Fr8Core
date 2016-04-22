using Data.Control;
using Data.Interfaces.Manifests;
using System;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;
using Hub.Services;
using System.Linq;
using System.Collections.Generic;
using Data.States;
using Data.Constants;
using Data.Crates;
using Data.Entities;
using Hub.Managers;
using Utilities;

namespace terminalFr8Core.Actions
{
    public class FindObjectsThatMatchIncomingMessage_v1 : EnhancedTerminalActivity<FindObjectsThatMatchIncomingMessage_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList IncomingTextSelector { get; set; }

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
                IncomingTextSelector = new DropDownList
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
                    Value = "<i class=\"fa fa-info-circle\" />Multiple values should be separated with commas. If no value is specified then first property will be used"
                };
                Controls.Add(DataSourceSelector);
                Controls.Add(IncomingTextSelector);
                Controls.Add(KeywordPropertiesSource);
                Controls.Add(KeywordPropertiesSourceDescription);
            }
        }

        private static readonly string[] StopwordsList = new[] { "the", "company", "co", "inc", "incorporate", "a", "an" };

        public FindObjectsThatMatchIncomingMessage_v1() : base(false)
        {
            ActivityName = "Find Objects That Match Incoming Message";
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            var activityTemplates = await HubCommunicator.GetActivityTemplates(ActivityTemplate.TableDataGeneratorTag, CurrentFr8UserId);
            activityTemplates.Sort((x, y) => x.Name.CompareTo(y.Name));
            ConfigurationControls.DataSourceSelector.ListItems = activityTemplates
                                                                 .Select(x => new ListItem { Key = x.Label, Value = x.Id.ToString() })
                                                                 .ToList();
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            //Remove child activity if its not specified or add it if is not yet added
            if (string.IsNullOrEmpty(ConfigurationControls.DataSourceSelector.Value))
            {
                await HubCommunicator.DeleteExistingChildNodesFromActivity(CurrentActivity.Id, CurrentFr8UserId);
                CurrentActivity.ChildNodes.Clear();
                PreviousSelectedDataSourceId = null;
                CachedData = null;
            }
            else if (string.IsNullOrEmpty(PreviousSelectedDataSourceId) || PreviousSelectedDataSourceId != ConfigurationControls.DataSourceSelector.Value)
            {
                var activityTemplate = await GetActivityTemplate(Guid.Parse(ConfigurationControls.DataSourceSelector.Value));
                await HubCommunicator.DeleteExistingChildNodesFromActivity(CurrentActivity.Id, CurrentFr8UserId);
                CurrentActivity.ChildNodes.Clear();
                await AddAndConfigureChildActivity(CurrentActivity, activityTemplate, order: 1);
                PreviousSelectedDataSourceId = ConfigurationControls.DataSourceSelector.Value;
                CachedData = null;
            }
        }

        protected override async Task RunCurrentActivity()
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
            CurrentPayloadStorage.Remove(dataToFilter);
            var dataProperties = GetDataProperties(dataToFilter.Content);
            var result = FilterData(IncomingText, dataToFilter, dataProperties);
            CurrentPayloadStorage.Add(result);
            IsInitialRun = true;
            //We don't want to run child activity again as we already used its data
            RequestSkipChildren();
        }

        protected override Task RunChildActivities()
        {
            //We want to run our activity in filtering mode after child activity is completed
            RequestJumpToActivity(CurrentActivity.Id);
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
            //If there is nothing to filter OR there are no keywords we just return the original data from child activity
            if (dataProperties?.Count == 0 || string.IsNullOrWhiteSpace(message))
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
            return Crate<StandardTableDataCM>.FromContent(data.Label, result, data.Availability);
        }

        private string[] GetPropertyValuesFromDataRow(TableRowDTO dataRow, HashSet<string> dataProperties)
        {
            var result = dataRow.Row
                                .Select(x => x.Cell)
                                .Where(x => dataProperties.Contains(x.Key))
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
            if (!string.IsNullOrWhiteSpace(SpecifiedDataProperties))
            {
                return new HashSet<string>(SpecifiedDataProperties.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()), StringComparer.InvariantCultureIgnoreCase);
            }
            return new HashSet<string>(new[] { data.Table[0].Row[0].Cell.Key }, StringComparer.InvariantCultureIgnoreCase);
        }

        private Crate<StandardTableDataCM> NewData
        {
            get
            {
                //TODO: we expect that activities that generate table data have to publish crate description with this data
                //If this assumption won't be accepted then we'll have to compare payload before and after child activity is executed to get the data generated by child activity
                var childActivityStorage = CrateManager.GetStorage((ActivityDO)CurrentActivity.GetOrderedChildren()[0]);
                var dataDescription = childActivityStorage.FirstCrateOrDefault<CrateDescriptionCM>()
                                                         ?.Content
                                                          .CrateDescriptions
                                                          .FirstOrDefault(x => x.ManifestId == (int)MT.StandardTableData);
                if (dataDescription == null)
                {
                    return null;
                }
                return CurrentPayloadStorage.FirstCrateOrDefault<StandardTableDataCM>(x => x.Label == dataDescription.Label);
            }
        }

        private Crate<StandardTableDataCM> CachedData
        {
            get
            {
                return CurrentActivityStorage.FirstCrateOrDefault<StandardTableDataCM>();
            }
            set
            {
                if (value == null)
                {
                    CurrentActivityStorage.Remove<StandardTableDataCM>();
                }
                else
                {
                    CurrentActivityStorage.Remove<StandardTableDataCM>();
                    CurrentActivityStorage.Add(value);
                }
            }
        }

        private bool IsInitialRun
        {
            get { return !CurrentPayloadStorage.Any(x => x.Label == CurrentActivity.Id.ToString()); }
            set
            {
                if (value)
                {
                    CurrentPayloadStorage.RemoveByLabel(CurrentActivity.Id.ToString());
                }
                else
                {
                    CurrentPayloadStorage.ReplaceByLabel(Crate<StandardPayloadDataCM>.FromContent(CurrentActivity.Id.ToString(), new StandardPayloadDataCM()));
                }
            }
        }

        private void ValidateRunPrerequisites()
        {
            if (string.IsNullOrEmpty(SelectedDataSourceActivityId))
            {
                throw new ActivityExecutionException("Data source is not specified", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            if (CurrentActivity.ChildNodes.Count == 0)
            {
                throw new ActivityExecutionException("Data source activity is missing");
            }
            var datasourceActivity = (ActivityDO)CurrentActivity.GetOrderedChildren()[0];
            if (datasourceActivity.ActivityTemplateId != Guid.Parse(SelectedDataSourceActivityId))
            {
                throw new ActivityExecutionException("Data source activity is other than specified in data source");
            }
        }
        //Wrappers for control properties
        private string IncomingText { get { return CurrentPayloadStorage.RetrieveValue(ConfigurationControls.IncomingTextSelector.selectedKey); } }

        private string SelectedDataSourceActivityId { get { return ConfigurationControls.DataSourceSelector.Value; } }
        //Untill we decide on caching strategy we won't use cache
        public bool CachedDataIsOld { get { return true; } }

        private string SpecifiedDataProperties { get { return ConfigurationControls.KeywordPropertiesSource.Value; } }


        #endregion
    }
}