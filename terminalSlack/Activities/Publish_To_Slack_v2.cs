using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Manifests;
using Fr8Data.States;
using terminalSlack.Interfaces;
using terminalSlack.Services;
using TerminalBase.BaseClasses;

namespace terminalSlack.Actions
{
    public class Publish_To_Slack_v2 : EnhancedTerminalActivity<Publish_To_Slack_v2.ActivityUi>
    {
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

        public Publish_To_Slack_v2() : base(true)
        {
            _slackIntegration = new SlackIntegration();
        }

        protected override async Task Initialize(CrateSignaller crateSignaller)
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
            ConfigurationControls.ChannelSelector.ListItems = channelsAndUsersList;
        }

        protected override Task Configure(CrateSignaller crateSignaller)
        {
            //No extra config is required
            return Task.FromResult(0);
        }

        protected override async Task RunCurrentActivity()
        {
            var channel = ConfigurationControls.ChannelSelector.Value;
            if (string.IsNullOrEmpty(channel))
            {
                throw new ActivityExecutionException("Channel or user is not specified");
            }
            var message = ConfigurationControls.MessageSource.GetValue(CurrentPayloadStorage);
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ActivityExecutionException("Can't post empty message to Slack");
            }
            var success = await _slackIntegration.PostMessageToChat(AuthorizationToken.Token, channel, message).ConfigureAwait(false);
            if (!success)
            {
                throw new ActivityExecutionException("Failed to post message to Slack");
            }
        }
    }
}