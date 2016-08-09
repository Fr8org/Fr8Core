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
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Services;

namespace terminalSalesforce.Actions
{
    public class Post_To_Chatter_v1 : BaseSalesforceTerminalActivity<Post_To_Chatter_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("E2250022-FA40-4FCF-9CDD-130DF6DD1984"),
            Version = "1",
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
            public RadioButtonGroup FeedParentSelectionGroup { get; set; }

            public RadioButtonOption UseUserOrGroupOption  { get; set; }

            public DropDownList UserOrGroupSelector { get; set; }

            public RadioButtonOption UseUpstreamFeedParentIdOption { get; set; }

            public TextSource FeedParentIdSource { get; set; }
            
            public TextSource FeedTextSource { get; set; }

            public ActivityUi() : this(new UiBuilder()) { }

            public ActivityUi(UiBuilder uiBuilder)
            {
                FeedParentIdSource = uiBuilder.CreateSpecificOrUpstreamValueChooser("Feed Parent Id", nameof(FeedParentIdSource), requestUpstream: true, availability: AvailabilityType.RunTime);
                UseUpstreamFeedParentIdOption = new RadioButtonOption
                {
                    Name = nameof(UseUpstreamFeedParentIdOption),
                    Value = "Use this upstream value as Feed Parent Id",
                    Controls = new List<ControlDefinitionDTO> { FeedParentIdSource }
                };
                UserOrGroupSelector = new DropDownList
                {
                    Name = nameof(UserOrGroupSelector),
                    Label = "Post to which Chatter Person or Group?"
                };
                UseUserOrGroupOption = new RadioButtonOption
                {
                    Name = nameof(UseUserOrGroupOption),
                    Value = "Specify user or group",
                    Controls = new List<ControlDefinitionDTO> { UserOrGroupSelector }
                };
                FeedParentSelectionGroup = new RadioButtonGroup
                {
                    Name = nameof(FeedParentSelectionGroup),
                    GroupName = nameof(FeedParentSelectionGroup),
                    Radios = new List<RadioButtonOption> {  UseUserOrGroupOption, UseUpstreamFeedParentIdOption }
                };
                Controls.Add(FeedParentSelectionGroup);
                FeedTextSource = uiBuilder.CreateSpecificOrUpstreamValueChooser("Feed Text", nameof(FeedTextSource), requestUpstream: true, availability: AvailabilityType.RunTime);
                Controls.Add(FeedTextSource);
            }         
        }

        public const string PostedFeedCrateLabel = "Posted Salesforce Feed";

        private readonly ISalesforceManager _salesforceManager;

        public Post_To_Chatter_v1(ICrateManager crateManager, ISalesforceManager salesforceManager)
            : base(crateManager)
        {
            _salesforceManager = salesforceManager;
        }

        public override async Task Initialize()
        {
            ActivityUI.UseUserOrGroupOption.Selected = true;
            ActivityUI.UserOrGroupSelector.ListItems = (await _salesforceManager.GetUsersAndGroups(AuthorizationToken)).Select(x => new ListItem { Key = x.Key, Value = x.Value }).ToList();
            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(PostedFeedCrateLabel);            
        }

        public override Task FollowUp()
        {
            //No configuration is required
            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            var feedText = ActivityUI.FeedTextSource.TextValue;
            if (string.IsNullOrEmpty(feedText))
            {
                throw new ActivityExecutionException("Can't post empty message to chatter");
            }
            if (!ActivityUI.UseUpstreamFeedParentIdOption.Selected && !ActivityUI.UseUserOrGroupOption.Selected)
            {
                throw new ActivityExecutionException("Feed parent Id value source is not specified");
            }
            var feedParentId = string.Empty;
            if (ActivityUI.UseUserOrGroupOption.Selected)
            {
                feedParentId = ActivityUI.UserOrGroupSelector.Value;
                if (string.IsNullOrEmpty(feedParentId))
                {
                    throw new ActivityExecutionException("User or group is not specified");
                }
            }
            if (ActivityUI.UseUpstreamFeedParentIdOption.Selected)
            {
                feedParentId = ActivityUI.FeedParentIdSource.TextValue;
                if (string.IsNullOrEmpty(feedParentId))
                {
                    throw new ActivityExecutionException("Upstream crates doesn't contain value for feed parent Id");
                }
            }
            var result = await _salesforceManager.PostToChatter(StripHTML(feedText), feedParentId, AuthorizationToken);
            if (string.IsNullOrEmpty(result))
            {
                throw new ActivityExecutionException("Failed to post to chatter due to Salesforce API error");
            }
            Payload.Add(Crate.FromContent(PostedFeedCrateLabel, new StandardPayloadDataCM(new KeyValueDTO("FeedID", result))));
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        
    }
}