using StructureMap;
using System.Threading.Tasks;
using TerminalBase.BaseClasses;
using terminalSalesforce.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using Data.Crates;
using Data.Control;
using Data.Interfaces.Manifests;
using System.Collections.Generic;
using System.Linq;
using Data.States;
using Newtonsoft.Json;

namespace terminalSalesforce.Actions
{
    public class Post_To_Chatter_v2 : EnhancedTerminalActivity<Post_To_Chatter_v2.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextSource FeedTextSource { get; set; }

            public RadioButtonGroup ChatterSelectionGroup { get; set; }

            public RadioButtonOption QueryForChatterOption { get; set; }

            public DropDownList ChatterSelector { get; set; }

            public QueryBuilder ChatterFilter { get; set; }

            public RadioButtonOption UseIncomingChatterIdOption { get; set; }

            public DropDownList IncomingChatterIdSelector { get; set; }

            public ActivityUi() : this(new UiBuilder()) { }

            public ActivityUi(UiBuilder uiBuilder)
            {
                FeedTextSource = uiBuilder.CreateSpecificOrUpstreamValueChooser("Chatter Message", nameof(FeedTextSource), requestUpstream: true, availability: AvailabilityType.RunTime);
                ChatterSelector = new DropDownList
                {
                    Name = nameof(ChatterSelector),
                    Label = "Get which object?",
                    Required = true,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                ChatterFilter = new QueryBuilder
                {
                    Name = nameof(ChatterFilter),
                    Label = "Meeting which conditions?",
                    Required = true,
                    Source = new FieldSourceDTO
                    {
                        Label = QueryFilterCrateLabel,
                        ManifestType = CrateManifestTypes.StandardQueryFields
                    }
                };
                QueryForChatterOption = new RadioButtonOption
                {
                    Name = nameof(QueryForChatterOption),
                    Value = "Query for chatter objects",
                    Controls = new List<ControlDefinitionDTO> { ChatterSelector, ChatterFilter }
                };
                IncomingChatterIdSelector = new DropDownList
                {
                    Name = nameof(IncomingChatterIdSelector),
                    Source = new FieldSourceDTO
                    {
                        AvailabilityType = AvailabilityType.RunTime,
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                        RequestUpstream = true
                    }
                };
                UseIncomingChatterIdOption = new RadioButtonOption
                {
                    Name = nameof(UseIncomingChatterIdOption),
                    Value = "Use this incoming value as chatter Id",
                    Controls = new List<ControlDefinitionDTO> { IncomingChatterIdSelector }
                };
                ChatterSelectionGroup = new RadioButtonGroup
                {
                    Name = nameof(ChatterSelectionGroup),
                    GroupName = nameof(ChatterSelectionGroup),
                    Label = "Which chatter to post to?",
                    Radios = new List<RadioButtonOption> { QueryForChatterOption, UseIncomingChatterIdOption }
                };
                Controls.Add(FeedTextSource);
                Controls.Add(ChatterSelectionGroup);
            }
        }

        public const string QueryFilterCrateLabel = "Queryable Criteria";

        public const string PostedFeedCrateLabel = "Posted Salesforce Feed";

        public const string SalesforceObjectFieldsCrateLabel = "Salesforce Object Fields";

        private readonly ISalesforceManager _salesforceManager;

        public Post_To_Chatter_v2() : base(true)
        {
            _salesforceManager = ObjectFactory.GetInstance<ISalesforceManager>();
            ActivityName = "Post to Chatter";
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            IsPostingToQueryiedChatter = true;
            AvailableChatters = _salesforceManager.GetObjectProperties().Select(x => new ListItem { Key = x.Key, Value = x.Value }).ToList();
            runtimeCrateManager.MarkAvailableAtRuntime<StandardPayloadDataCM>(PostedFeedCrateLabel);
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            //If Salesforce object is empty then we should clear filters as they are no longer applicable
            if (string.IsNullOrEmpty(SelectedChatter))
            {
                CurrentActivityStorage.RemoveByLabel(QueryFilterCrateLabel);
                CurrentActivityStorage.RemoveByLabel(SalesforceObjectFieldsCrateLabel);
                this[nameof(SelectedChatter)] = SelectedChatter;
                return;
            }
            //If the same object is selected we shouldn't do anything
            if (SelectedChatter == this[nameof(SelectedChatter)])
            {
                return;
            }
            //Prepare new query filters from selected object properties
            var selectedObjectProperties = await _salesforceManager.GetFields(SelectedChatter, AuthorizationToken);
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
            this[nameof(SelectedChatter] = SelectedChatter;
            //Publish information for downstream activities
            runtimeCrateManager.MarkAvailableAtRuntime<StandardTableDataCM>(PostedFeedCrateLabel);
        }

        protected override async Task RunCurrentActivity()
        {
            var feedText = FeedText;
            if (string.IsNullOrEmpty(feedText))
            {
                throw new ActivityExecutionException("Can't post empty message to chatter");
            }
            if (!IsPostingToQueryiedChatter && !IsUsingIncomingChatterId)
            {
                throw new ActivityExecutionException("Chatter Id value source is not specified");
            }
            if (IsPostingToQueryiedChatter)
            {
                var chatters = await _salesforceManager.Query(SelectedChatter,
                                                                     new[] { "Id" },
                                                                     ParseConditionToText(JsonConvert.DeserializeObject<List<FilterConditionDTO>>(ChatterFilter)),
                                                                     AuthorizationToken);
                foreach (var chatter in  )
                feedParentId = ConfigurationControls.ChatterFilter.Value;
                if (string.IsNullOrEmpty(feedParentId))
                {
                    throw new ActivityExecutionException("User or group is not specified");
                }
            }
            if (ConfigurationControls.UseIncomingChatterIdOption.Selected)
            {
                feedParentId = ConfigurationControls.IncomingChatterIdSelector.GetValue(CurrentPayloadStorage);
                if (string.IsNullOrEmpty(feedParentId))
                {
                    throw new ActivityExecutionException("Upstream crates doesn't contain value for feed parent Id");
                }
            }
            var result = await _salesforceManager.PostFeedTextToChatterObject(feedText, feedParentId, AuthorizationToken);
            if (string.IsNullOrEmpty(result))
            {
                throw new ActivityExecutionException("Failed to post to chatter due to Salesforce API error");
            }
            CurrentPayloadStorage.Add(Crate.FromContent(PostedFeedCrateLabel, new StandardPayloadDataCM(new FieldDTO("FeedID", result))));
        }

        #region Controls properties wrappers

        private string SelectedChatter { get { return ConfigurationControls.ChatterSelector.selectedKey; } }

        private bool IsPostingToQueryiedChatter
        {
            get { return ConfigurationControls.QueryForChatterOption.Selected; }
            set { ConfigurationControls.QueryForChatterOption.Selected = value; }
        }

        private bool IsUsingIncomingChatterId
        {
            get { return ConfigurationControls.UseIncomingChatterIdOption.Selected; }
            set { ConfigurationControls.UseIncomingChatterIdOption.Selected = value; }
        }

        private List<ListItem> AvailableChatters
        {
            get { return ConfigurationControls.ChatterSelector.ListItems; }
            set { ConfigurationControls.ChatterSelector.ListItems = value; }
        }

        private string FeedText { get { return ConfigurationControls.FeedTextSource.GetValue(CurrentPayloadStorage); } }

        private string ChatterFilter { get { return ConfigurationControls.ChatterFilter.Value; } }


        #endregion
    }
}