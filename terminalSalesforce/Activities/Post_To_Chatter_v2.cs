using System;
using StructureMap;
using System.Threading.Tasks;
using terminalSalesforce.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Helpers;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Services;
using log4net;
using Newtonsoft.Json;
using ServiceStack;

namespace terminalSalesforce.Actions
{
    public class Post_To_Chatter_v2 : BaseSalesforceTerminalActivity<Post_To_Chatter_v2.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("5052fc23-c867-4d5a-8fbb-b6b64b5ad688"),
            Version = "2",
            Name = "Post_To_Chatter",
            Label = "Post To Salesforce Chatter",
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Forward,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;


        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextSource FeedTextSource { get; set; }

            public RadioButtonGroup ChatterSelectionGroup { get; set; }

            public RadioButtonOption QueryForChatterOption { get; set; }

            public DropDownList ChatterSelector { get; set; }

            public QueryBuilder ChatterFilter { get; set; }

            public RadioButtonOption UseIncomingChatterIdOption { get; set; }

            public UpstreamFieldChooser IncomingChatterIdSelector { get; set; }

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
                IncomingChatterIdSelector = new UpstreamFieldChooser
                {
                    Name = nameof(IncomingChatterIdSelector),
                    Source = new FieldSourceDTO
                    {
                        AvailabilityType = AvailabilityType.RunTime,
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
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

        private static readonly ILog Logger = Fr8.Infrastructure.Utilities.Logging.Logger.GetCurrentClassLogger();
            

        private readonly ISalesforceManager _salesforceManager;
       

        public Post_To_Chatter_v2(ICrateManager crateManager, ISalesforceManager salesforceManager)
            : base(crateManager)
        {
            _salesforceManager = salesforceManager;
        }

        public override async Task Initialize()
        {
            IsPostingToQueryiedChatter = true;
            AvailableChatters = _salesforceManager.GetSalesforceObjectTypes(filterByProperties: SalesforceObjectProperties.HasChatter).Select(x => new ListItem {Key = x.Name, Value = x.Label}).ToList();
            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(PostedFeedCrateLabel).AddField(FeedIdKeyName);
        }

        public override async Task FollowUp()
        {
            //If Salesforce object is empty then we should clear filters as they are no longer applicable
            if (string.IsNullOrEmpty(SelectedChatter))
            {
                Storage.RemoveByLabel(QueryFilterCrateLabel);
                Storage.RemoveByLabel(SalesforceObjectFieldsCrateLabel);
                this[nameof(SelectedChatter)] = SelectedChatter;
                return;
            }
            //If the same object is selected we shouldn't do anything
            if (SelectedChatter == this[nameof(SelectedChatter)])
            {
                return;
            }
            //Prepare new query filters from selected object properties
            var selectedObjectProperties = await _salesforceManager.GetProperties(SelectedChatter.ToEnum<SalesforceObjectType>(), AuthorizationToken,false,PostedFeedCrateLabel);
            var queryFilterCrate = Crate<FieldDescriptionsCM>.FromContent(
                QueryFilterCrateLabel,
                new FieldDescriptionsCM(selectedObjectProperties));
            Storage.ReplaceByLabel(queryFilterCrate);

            var objectPropertiesCrate = Crate<FieldDescriptionsCM>.FromContent(
                SalesforceObjectFieldsCrateLabel,
                new FieldDescriptionsCM(selectedObjectProperties));
            Storage.ReplaceByLabel(objectPropertiesCrate);
            this[nameof(SelectedChatter)] = SelectedChatter;
            //Publish information for downstream activities
            CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>(PostedFeedCrateLabel);
        }


        protected override Task Validate()
        {
            ValidationManager.ValidateTextSourceNotEmpty(ActivityUI.FeedTextSource, "Can't post empty message to chatter");

            if (!IsPostingToQueryiedChatter && !IsUsingIncomingChatterId)
            {
                ValidationManager.SetError("Chatter Id value source is not specified", ActivityUI.ChatterSelectionGroup);
            }

            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            var feedText = FeedText;
            
            if (IsPostingToQueryiedChatter)
            {
                try
                {
                    var chatters = await _salesforceManager.Query(SelectedChatter.ToEnum<SalesforceObjectType>(),
                                                              new[] { new FieldDTO("Id") },
                                                              FilterConditionHelper.ParseConditionToText(JsonConvert.DeserializeObject<List<FilterConditionDTO>>(ChatterFilter)),
                                                              AuthorizationToken);
                    var tasks = new List<Task<string>>(chatters.Table.Count);
                    foreach (var chatterId in chatters.DataRows.Select(x => x.Row[0].Cell.Value))
                    {
                        Logger.Info($"Posting message to chatter id: {chatterId}");

                        tasks.Add(_salesforceManager.PostToChatter(StripHTML(feedText), chatterId, AuthorizationToken).ContinueWith(x =>
                        {
                            Logger.Info($"Posting message to chatter succeded with feedId: {x.Result}");
                            return x.Result;
                        }));
                    }
                    await Task.WhenAll(tasks);
                    //If we did not find any chatter object we don't fail activity execution but rather returns empty list and inform caller about it 
                    if (!chatters.HasDataRows)
                    {
                        Logger.Info("No salesforce objects were found to use as chatter id.");
                        Success($"No {SelectedChatter} that satisfies specified conditions were found. No message were posted");
                    }
                    else
                    {
                        var resultPayload = new StandardPayloadDataCM();
                        resultPayload.PayloadObjects.AddRange(tasks.Select(x => new PayloadObjectDTO(new KeyValueDTO(FeedIdKeyName, x.Result))));
                        Payload.Add(Crate<StandardPayloadDataCM>.FromContent(PostedFeedCrateLabel, resultPayload));
                    }
                }
                catch (Exception ex)
                {
                    RaiseError(ex.Message);
                    return;
                }
            }
            else
            {
                var incomingChatterId = IncomingChatterId;
                if (string.IsNullOrWhiteSpace(incomingChatterId))
                {
                    throw new ActivityExecutionException("Upstream crates doesn't contain value for feed parent Id");
                }

                Logger.Info($"Posting message to chatter id: {incomingChatterId}");

                var feedId = await _salesforceManager.PostToChatter(StripHTML(feedText), incomingChatterId, AuthorizationToken);

                Logger.Info($"Posting message to chatter succeded with feedId: {feedId}");

                Payload.Add(Crate.FromContent(PostedFeedCrateLabel, new StandardPayloadDataCM(new KeyValueDTO(FeedIdKeyName, feedId))));
            }
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        #region Controls properties wrappers

        private string SelectedChatter => ActivityUI.ChatterSelector.selectedKey;

        private bool IsPostingToQueryiedChatter
        {
            get { return ActivityUI.QueryForChatterOption.Selected; }
            set { ActivityUI.QueryForChatterOption.Selected = value; }
        }

        private bool IsUsingIncomingChatterId => ActivityUI.UseIncomingChatterIdOption.Selected;

        private List<ListItem> AvailableChatters
        {
            set { ActivityUI.ChatterSelector.ListItems = value; }
        }

        private string FeedText => ActivityUI.FeedTextSource.TextValue;

        private string ChatterFilter => ActivityUI.ChatterFilter.Value;

        private string IncomingChatterId => ActivityUI.IncomingChatterIdSelector.Value;

        #endregion
    }
}