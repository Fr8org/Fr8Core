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

namespace terminalSalesforce.Actions
{
    public class Post_To_Chatter_v1 : BaseSalesforceTerminalActivity<Post_To_Chatter_v1.ActivityUi>
    {
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

        public Post_To_Chatter_v1()
        {
            _salesforceManager = ObjectFactory.GetInstance<ISalesforceManager>();
            ActivityName = "Post to Chatter";
        }

        protected override async Task Initialize(CrateSignaller crateSignaller)
        {
            ConfigurationControls.UseUserOrGroupOption.Selected = true;
            ConfigurationControls.UserOrGroupSelector.ListItems = (await _salesforceManager.GetUsersAndGroups(AuthorizationToken)).Select(x => new ListItem { Key = x.Key, Value = x.Value }).ToList();
            crateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(PostedFeedCrateLabel);            
        }

        protected override Task Configure(CrateSignaller crateSignaller)
        {
            //No configuration is required
            return Task.FromResult(0);
        }

        protected override async Task RunCurrentActivity()
        {
            var feedText = ConfigurationControls.FeedTextSource.GetValue(CurrentPayloadStorage);
            if (string.IsNullOrEmpty(feedText))
            {
                throw new ActivityExecutionException("Can't post empty message to chatter");
            }
            if (!ConfigurationControls.UseUpstreamFeedParentIdOption.Selected && !ConfigurationControls.UseUserOrGroupOption.Selected)
            {
                throw new ActivityExecutionException("Feed parent Id value source is not specified");
            }
            var feedParentId = string.Empty;
            if (ConfigurationControls.UseUserOrGroupOption.Selected)
            {
                feedParentId = ConfigurationControls.UserOrGroupSelector.Value;
                if (string.IsNullOrEmpty(feedParentId))
                {
                    throw new ActivityExecutionException("User or group is not specified");
                }
            }
            if (ConfigurationControls.UseUpstreamFeedParentIdOption.Selected)
            {
                feedParentId = ConfigurationControls.FeedParentIdSource.GetValue(CurrentPayloadStorage);
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
            CurrentPayloadStorage.Add(Crate.FromContent(PostedFeedCrateLabel, new StandardPayloadDataCM(new FieldDTO("FeedID", result))));
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
    }
}