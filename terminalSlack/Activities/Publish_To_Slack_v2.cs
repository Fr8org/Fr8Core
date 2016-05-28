using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using terminalSlack.Interfaces;
using terminalSlack.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Errors;
using TerminalBase.Infrastructure;
using TerminalBase.Services;

namespace terminalSlack.Activities
{
    public class Publish_To_Slack_v2 : EnhancedTerminalActivity<Publish_To_Slack_v2.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Publish_To_Slack",
            Label = "Publish To Slack",
            Tags = "Notifier",
            Category = ActivityCategory.Forwarders,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            Version = "2",
            WebService = TerminalData.WebServiceDTO,
            MinPaneWidth = 330
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList ChannelSelector { get; set; }

            public TextSource MessageSource { get; set; }

            public ActivityUi(UiBuilder uiBuilder)
            {
                ChannelSelector = new DropDownList { Label = "Select Slack Channel" };
                MessageSource = uiBuilder.CreateSpecificOrUpstreamValueChooser("Message", nameof(MessageSource), requestUpstream: true, availability: AvailabilityType.RunTime);
                Controls.Add(ChannelSelector);
                Controls.Add(MessageSource);
            }
        }

        private readonly ISlackIntegration _slackIntegration;


        public Publish_To_Slack_v2(ICrateManager crateManager)
            : base(true, crateManager)
        {
            _slackIntegration = new SlackIntegration();
        }
        public override async Task Initialize()
        {
            var usersTask = _slackIntegration.GetUserList(AuthorizationToken.Token);
            var channelsTask = _slackIntegration.GetChannelList(AuthorizationToken.Token);
            await Task.WhenAll(usersTask, channelsTask).ConfigureAwait(false);
            var channelsAndUsersList = new List<ListItem>(usersTask.Result.Count + channelsTask.Result.Count);
            channelsAndUsersList.AddRange(channelsTask.Result
                                                      .OrderBy(x => x.Key)
                                                      .Select(x => new ListItem { Key = $"#{x.Key}", Value = x.Value }));
            channelsAndUsersList.AddRange(usersTask.Result
                                                   .OrderBy(x => x.Key)
                                                   .Select(x => new ListItem { Key = $"@{x.Key}", Value = x.Value }));
            ActivityUI.ChannelSelector.ListItems = channelsAndUsersList;
        }

        public override Task FollowUp()
        {
            //No extra config is required
            return Task.FromResult(0);
        }

        protected override Task Validate()
        {
            if (string.IsNullOrEmpty(ActivityUI.ChannelSelector.Value))
            {
                ValidationManager.SetError("Channel or user is not specified", ActivityUI.ChannelSelector);
            }

            ValidationManager.ValidateTextSourceNotEmpty(ActivityUI.MessageSource, "Can't post empty message to Slack");

            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            var channel = ActivityUI.ChannelSelector.Value;
            var message = ActivityUI.MessageSource.GetValue(Payload);
            var success = await _slackIntegration.PostMessageToChat(AuthorizationToken.Token, channel, message).ConfigureAwait(false);
            if (!success)
            {
                throw new ActivityExecutionException("Failed to post message to Slack");
            }
        }
    }
}