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
using System.Text;

namespace terminalFr8Core.Actions
{
    public class KeywordsFilter_v1 : EnhancedTerminalActivity<KeywordsFilter_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextSource MessageTextSource { get; set; }

            public DropDownList DataSourceSelector { get; set; }

            public CheckBox IsCachingDataSourceData { get; set; }

            public TextBox KeywordPropertiesSource { get; set; }

            public TextBlock KeywordPropertiesSourceDescription { get; set; }

            public CheckBox IsRemovingStopwords { get; set; }

            public TextBox StopwordsSource { get; set; }

            public TextBlock StopwordsSourceDescription { get; set; }

            public ActivityUi() : this(new UiBuilder())
            {
            }

            public ActivityUi(UiBuilder uiBuilder)
            {
                MessageTextSource = uiBuilder.CreateSpecificOrUpstreamValueChooser("Message", nameof(MessageTextSource), requestUpstream: true, availability: AvailabilityType.RunTime);
                DataSourceSelector = new DropDownList
                {
                    Name = nameof(DataSourceSelector),
                    Label = "Keywords source",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                IsCachingDataSourceData = new CheckBox
                {
                    Name = nameof(IsCachingDataSourceData),
                    Label = "Get data on first run only",
                };
                KeywordPropertiesSource = new TextBox
                {
                    Name = nameof(KeywordPropertiesSource),
                    Label = "Keywords source properties"
                };
                KeywordPropertiesSourceDescription = new TextBlock
                {
                    Name = nameof(KeywordPropertiesSourceDescription),
                    Value = "<i class=\"fa fa-info-circle\" />Multiple values should be separated with commas. If no value is specified then first property will be used"
                };
                IsRemovingStopwords = new CheckBox
                {
                    Name = nameof(IsRemovingStopwords),
                    Label = "(Optional)Remove following stopwords"
                };
                StopwordsSource = new TextBox
                {
                    Name = nameof(StopwordsSource),
                };
                StopwordsSourceDescription = new TextBlock
                {
                    Name = nameof(StopwordsSourceDescription),
                    Value = "<i class=\"fa fa-info-circle\" />Multiple values should be separated with commas"
                };
                Controls.Add(MessageTextSource);
                Controls.Add(DataSourceSelector);
                Controls.Add(IsCachingDataSourceData);
                Controls.Add(KeywordPropertiesSource);
                Controls.Add(KeywordPropertiesSourceDescription);
                Controls.Add(IsRemovingStopwords);
                Controls.Add(StopwordsSource);
                Controls.Add(StopwordsSourceDescription);
            }
        }

        public KeywordsFilter_v1() : base(false)
        {
            ActivityName = "Keywords Filter";
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
                PreviousSelectedActivity = null;
                CachedData = null;
            }
            else if (string.IsNullOrEmpty(PreviousSelectedActivity) || PreviousSelectedActivity != ConfigurationControls.DataSourceSelector.Value)
            {
                var activityTemplate = await GetActivityTemplate(Guid.Parse(ConfigurationControls.DataSourceSelector.Value));
                await HubCommunicator.DeleteExistingChildNodesFromActivity(CurrentActivity.Id, CurrentFr8UserId);
                CurrentActivity.ChildNodes.Clear();
                await AddAndConfigureChildActivity(CurrentActivity, activityTemplate, order: 1);
                PreviousSelectedActivity = ConfigurationControls.DataSourceSelector.Value;
                CachedData = null;
            }
        }

        private string PreviousSelectedActivity
        {
            get { return this[nameof(ActivityUi.DataSourceSelector)]; }
            set { this[nameof(ActivityUi.DataSourceSelector)] = value; }
        }

        protected override async Task RunCurrentActivity()
        {
            if (IsInitialRun)
            {
                ValidateRunPrerequisites();
                if (!IsUsingCachedData || CachedData == null)
                {
                    //If we don't want to use cached data or we don't have it then we let child activity to generate new data
                    return;
                }
            }
            //Here we come after child activity is executed OR we want to use cached data and we have it
            var keywordsSource = GetKeywordsSource();
            if (keywordsSource == null)
            {
                throw new ActivityExecutionException($"Data source activity didn't generated {MT.StandardTableData.GetEnumDisplayName()} crate");
            }
            //If child activity did ran this time we need to remove the data its produced from payload (as it represents unfiltered data)
            CurrentPayloadStorage.Remove(keywordsSource);
            var keywordsSourceProperties = GetKeywordsSourceProperties(keywordsSource.Content);
            var stopwordsToRemove = GetStopwordsToRemove();
            var result = FilterKeywordsSource(IncomingMessage, keywordsSource, keywordsSourceProperties, stopwordsToRemove);
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

        private string[] GetStopwordsToRemove()
        {
            if (!IsRemovingStopwords || string.IsNullOrWhiteSpace(SpecifiedStopwords))
            {
                return null;
            }
            return SpecifiedStopwords.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        }

        private Crate<StandardTableDataCM> FilterKeywordsSource(string message, Crate<StandardTableDataCM> keywordsSource, HashSet<string> keywordsSourceProperties, string[] stopwordsToRemove)
        {
            //If there is nothing to filter OR there are no keywords we just return the original data from child activity
            if (keywordsSourceProperties?.Count == 0 || string.IsNullOrWhiteSpace(message))
            {
                return keywordsSource;
            }
            var sourceTable = keywordsSource.Content;
            var result = new StandardTableDataCM { Table = new List<TableRowDTO>(), FirstRowHeaders = sourceTable.FirstRowHeaders };
            //Copy header row
            if (sourceTable.FirstRowHeaders && sourceTable.Table.Count > 0)
            {
                result.Table.Add(sourceTable.Table[0]);
            }
            foreach (var dataRow in sourceTable.DataRows)
            {
                var dataRowKeywords = GetKeywordsFromDataRow(dataRow, keywordsSourceProperties, stopwordsToRemove);
                var keywordIsFound = false;
                foreach (var keyword in dataRowKeywords)
                {
                    if (message.Contains(keyword, StringComparison.InvariantCultureIgnoreCase))
                    {
                        keywordIsFound = true;
                        break;
                    }
                }
                if (keywordIsFound)
                {
                    result.Table.Add(dataRow);
                }
            }
            return Crate<StandardTableDataCM>.FromContent(keywordsSource.Label, result, keywordsSource.Availability);            
        }

        private string[] GetKeywordsFromDataRow(TableRowDTO dataRow, HashSet<string> keywordsSourceProperties, string[] stopwordsToRemove)
        {
            var result = dataRow.Row
                                .Select(x => x.Cell)
                                .Where(x => keywordsSourceProperties.Contains(x.Key))
                                .Select(x => x.Value)
                                .Where(x => !string.IsNullOrWhiteSpace(x));
            if (stopwordsToRemove?.Length > 0)
            {
                result = result.Select(x => RemoveStopwords(x, stopwordsToRemove));
            }
            return result.ToArray();
        }

        private string RemoveStopwords(string value, string[] stopwordsToRemove)
        {
            //Currently this is straightforward removal of substrings
            foreach (var stopword in stopwordsToRemove)
            {
                var index = value.IndexOf(stopword, StringComparison.InvariantCultureIgnoreCase);
                value = value.Remove(index, stopword.Length);
            }
            return value.Replace("  ", " ").Trim();
        }

        private Crate<StandardTableDataCM> GetKeywordsSource()
        {
            var newData = NewData;
            Crate<StandardTableDataCM> actualData = null;
            if (IsUsingCachedData)
            {
                actualData = CachedData ?? (CachedData = NewData);
            }
            else
            {
                actualData = newData;
            }
            return actualData;
        }

        private HashSet<string> GetKeywordsSourceProperties(StandardTableDataCM keywordsSource)
        {
            if (!keywordsSource.HasDataRows || keywordsSource.Table[0].Row.Count == 0)
            {
                return null;
            }
            if (!string.IsNullOrWhiteSpace(SpecifiedKeywords))
            {
                return new HashSet<string>(SpecifiedKeywords.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()), StringComparer.InvariantCultureIgnoreCase);
            }
            return new HashSet<string>(new[] { keywordsSource.Table[0].Row[0].Cell.Key }, StringComparer.InvariantCultureIgnoreCase);            
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
        private string IncomingMessage { get { return ConfigurationControls.MessageTextSource.GetValue(CurrentPayloadStorage); } }

        private string SelectedDataSourceActivityId { get { return ConfigurationControls.DataSourceSelector.Value; } }

        private bool IsUsingCachedData { get { return ConfigurationControls.IsCachingDataSourceData.Selected; } }

        private string SpecifiedKeywords { get { return ConfigurationControls.KeywordPropertiesSource.Value; } }

        private bool IsRemovingStopwords { get { return ConfigurationControls.IsRemovingStopwords.Selected; } }

        private string SpecifiedStopwords { get { return ConfigurationControls.StopwordsSource.Value; } }

        #endregion
    }
}