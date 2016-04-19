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
                Controls.Add(KeywordPropertiesSourceDescription);
                Controls.Add(IsRemovingStopwords);
                Controls.Add(StopwordsSource);
                Controls.Add(StopwordsSourceDescription);
            }
        }

        protected internal const string CachedDataCrateLabelPrefix = "Cached data for ";

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
                if (!UseCachedData || CachedData == null)
                {
                    //If we don't want to use cached data or we don't have it then we let child activity to generate new data
                    return;
                }
            }
            //Here we come after child activity is executed OR we want to use cached data and we have it
            var keywordsSource = GetKeywordsSource();


        }

        private StandardTableDataCM GetKeywordsSource()
        {
            var newData = NewData;
            StandardTableDataCM actualData = null;
            if (UseCachedData)
            {
                actualData = CachedData ?? (CachedData = NewData);
            }
            else
            {
                actualData = newData;
            }
            if (actualData == null)
            {
                throw new ActivityExecutionException($"Data source activity didn't generated {MT.StandardTableData.GetEnumDisplayName()} crate");
            }
            return actualData;
        }

        protected override Task RunChildActivities()
        {
            //We want to run our activity in filtering mode after child activity is completed
            RequestJumpToActivity(CurrentActivity.Id);
            IsInitialRun = false;
            return Task.FromResult(0);
        }

        private StandardTableDataCM NewData
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
                var result = CurrentPayloadStorage.FirstCrateOrDefault<StandardTableDataCM>(x => x.Label == dataDescription.Label)?.Content;
                return result;
            }
        }

        private bool UseCachedData { get { return ConfigurationControls.IsCachingDataSourceData.Selected; } }

        private StandardTableDataCM CachedData
        {
            get
            {
                var cachedData = CurrentActivityStorage.FirstCrateOrDefault<StandardTableDataCM>(x => x.Label.StartsWith(CachedDataCrateLabelPrefix));
                var cacheDataExists = cachedData != null;
                var cacheDataIsValid = cacheDataExists && cachedData.Label == CachedDataCrateLabel;
                if (cacheDataIsValid)
                {
                    return cachedData.Content;
                }
                CachedData = null;
                return null;
            }
            set
            {
                if (value == null)
                {
                    CurrentActivityStorage.Remove(x => x.Label.StartsWith(CachedDataCrateLabelPrefix));
                }
                else
                {
                    CurrentActivityStorage.ReplaceByLabel(Crate<StandardTableDataCM>.FromContent(CachedDataCrateLabel, value));
                }
            }
        }

        private string CachedDataCrateLabel
        {
            get { return $"{CachedDataCrateLabelPrefix} {ConfigurationControls.DataSourceSelector.Value}"; }
        }

        //If payload doesn't contain create which labeled with current activity Id then it is the initial run
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
            if (string.IsNullOrEmpty(ConfigurationControls.DataSourceSelector.Value))
            {
                throw new ActivityExecutionException("Data source is not specified", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            if (CurrentActivity.ChildNodes.Count == 0)
            {
                throw new ActivityExecutionException("Data source activity is missing");
            }
            var datasourceActivity = (ActivityDO)CurrentActivity.GetOrderedChildren()[0];
            if (datasourceActivity.ActivityTemplateId != Guid.Parse(ConfigurationControls.DataSourceSelector.Value))
            {
                throw new ActivityExecutionException("Data source activity is other than specified in data source");
            }
        }
    }
}