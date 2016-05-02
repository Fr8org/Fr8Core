using System;
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
using System.Text.RegularExpressions;
using Data.States;
using Newtonsoft.Json;
using ServiceStack;
using Data.Helpers;

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
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
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

        public const string PostedFeedPropertiesCrateLabel = "Posted Feeds";

        public const string FeedIdKeyName = "FeedId";

        private readonly ISalesforceManager _salesforceManager;

        public Post_To_Chatter_v2() : base(true)
        {
            _salesforceManager = ObjectFactory.GetInstance<ISalesforceManager>();
            ActivityName = "Post to Chatter";
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            IsPostingToQueryiedChatter = true;
            AvailableChatters = _salesforceManager.GetSalesforceObjectTypes(filterByProperties: SalesforceObjectProperties.HasChatter).Select(x => new ListItem { Key = x.Key, Value = x.Value }).ToList();
            runtimeCrateManager.MarkAvailableAtRuntime<StandardPayloadDataCM>(PostedFeedCrateLabel);
            CurrentActivityStorage.Add(Crate<FieldDescriptionsCM>.FromContent(PostedFeedPropertiesCrateLabel,
                                                                              new FieldDescriptionsCM(new FieldDTO(FeedIdKeyName, FeedIdKeyName, AvailabilityType.RunTime)),
                                                                              AvailabilityType.RunTime));
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
            var selectedObjectProperties = await _salesforceManager.GetProperties(SelectedChatter.ToEnum<SalesforceObjectType>(), AuthorizationToken);
            var queryFilterCrate = Crate<FieldDescriptionsCM>.FromContent(
                QueryFilterCrateLabel,
                new FieldDescriptionsCM(selectedObjectProperties),
                AvailabilityType.Configuration);
            CurrentActivityStorage.ReplaceByLabel(queryFilterCrate);

            var objectPropertiesCrate = Crate<FieldDescriptionsCM>.FromContent(
                SalesforceObjectFieldsCrateLabel,
                new FieldDescriptionsCM(selectedObjectProperties),
                AvailabilityType.RunTime);
            CurrentActivityStorage.ReplaceByLabel(objectPropertiesCrate);
            this[nameof(SelectedChatter)] = SelectedChatter;
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
                var chatters = await _salesforceManager.Query(SelectedChatter.ToEnum<SalesforceObjectType>(),
                                                              new[] { "Id" },
                                                              ParseConditionToText(JsonConvert.DeserializeObject<List<FilterConditionDTO>>(ChatterFilter)),
                                                              AuthorizationToken);
              
                var tasks = new List<Task<string>>(chatters.Table.Count);
                foreach (var chatterId in chatters.DataRows.Select(x => x.Row[0].Cell.Value))
                {
                    tasks.Add(_salesforceManager.PostToChatter(feedText, chatterId, AuthorizationToken));
                }
                await Task.WhenAll(tasks);
                //If we did not find any chatter object we don't fail activity execution but rather returns empty list and inform caller about it 
                if (!chatters.HasDataRows)
                {
                    Success($"No {SelectedChatter} that satisfies specified conditions were found. No message were posted");
                }
                var resultPayload = new StandardPayloadDataCM();
                resultPayload.PayloadObjects.AddRange(tasks.Select(x => new PayloadObjectDTO(new FieldDTO(FeedIdKeyName, x.Result))));
                CurrentPayloadStorage.Add(Crate<StandardPayloadDataCM>.FromContent(PostedFeedCrateLabel, new StandardPayloadDataCM()));
            }
            else
            {
                var incomingChatterId = IncomingChatterId;
                if (string.IsNullOrWhiteSpace(incomingChatterId))
                {
                    throw new ActivityExecutionException("Upstream crates doesn't contain value for feed parent Id");
                }
                var feedId = await _salesforceManager.PostToChatter(StripHTML(feedText), incomingChatterId, AuthorizationToken);
                CurrentPayloadStorage.Add(Crate.FromContent(PostedFeedCrateLabel, new StandardPayloadDataCM(new FieldDTO(FeedIdKeyName, feedId))));
            }
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
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

        private string IncomingChatterId { get { return CurrentPayloadStorage.FindField(ConfigurationControls.IncomingChatterIdSelector.selectedKey); } }

        #endregion
    }
}