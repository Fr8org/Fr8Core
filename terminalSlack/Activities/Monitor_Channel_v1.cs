using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using terminalSlack.Interfaces;
using terminalSlack.Services;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using System;
using ServiceStack.Text;

namespace terminalSlack.Actions
{
    public class Monitor_Channel_v1 : TerminalActivity<Monitor_Channel_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public RadioButtonGroup ChannelSelectionGroup { get; set; }

            public RadioButtonOption AllChannelsOption { get; set; }

            public RadioButtonOption SpecificChannelOption { get; set; }

            public DropDownList ChannelList { get; set; }

            public TextBlock ActivityDescription { get; set; }

            public ActivityUi()
            {
                AllChannelsOption = new RadioButtonOption
                {
                    Name = nameof(AllChannelsOption),
                    Value = "Monitor all channels",
                    Selected = true
                };
                ChannelList = new DropDownList
                {
                    Name = nameof(ChannelList),
                };
                SpecificChannelOption = new RadioButtonOption
                {
                    Name = nameof(SpecificChannelOption),
                    Controls = new List<ControlDefinitionDTO> { ChannelList },
                    Value = "Monitor specific channel"
                };
                ChannelSelectionGroup = new RadioButtonGroup
                {
                    Name = nameof(ChannelSelectionGroup),
                    GroupName = nameof(ChannelSelectionGroup),
                    Radios = new List<RadioButtonOption> { AllChannelsOption, SpecificChannelOption }
                };
                ActivityDescription = new TextBlock
                {
                    Name = nameof(ActivityDescription),
                    Value = @"Slack doesn't currently offer a way for us to automatically request events for this channel. 
                    To enable events to be sent to Fr8, do the following: </br>
                    <ol>
                        <li>Go to https://{yourteamname}.slack.com/services/new/outgoing-webhook. </li>
                        <li>Click 'Add Outgoing WebHooks Integration'</li>
                        <li>In the Outgoing WebHook form go to 'URL(s)' field fill the following address: 
                            <strong>https://terminalslack.fr8.co/terminals/terminalslack/events</strong>
                        </li>
                    </ol>"
                };
                Controls.Add(ChannelSelectionGroup);
                Controls.Add(ActivityDescription);
            }
        }

        public const string SlackMessagePropertiesCrateLabel = "Slack Message Properties";

        public const string ResultPayloadCrateLabel = "Slack Message";

        private readonly ISlackIntegration _slackIntegration;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("246DF538-3B7E-4D1B-B045-72021BAA0D2D"),
            Name = "Monitor_Channel",
            Label = "Monitor Channel",
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            Version = "1",
            MinPaneWidth = 330,
            Categories = new[]
            {
                ActivityCategories.Monitor,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        

        public Monitor_Channel_v1(ICrateManager crateManager, ISlackIntegration slackIntegration)
            : base(crateManager)
        {
            _slackIntegration = slackIntegration;
        }
      
        public override Task Run()
        {
            var incomingMessageContents = ExtractIncomingMessageContentFromPayload();
            var hasIncomingMessage = incomingMessageContents?.Count > 0;

            if (hasIncomingMessage)
            {
                var channelMatches = ActivityUI.AllChannelsOption.Selected
                    || string.IsNullOrEmpty(ActivityUI.ChannelList.selectedKey)
                    || ActivityUI.ChannelList.Value == incomingMessageContents.FirstOrDefault(x=>x.Key == "channel_id")?.Value;
                if (channelMatches)
                {
                    Payload.Add(Crate.FromContent(ResultPayloadCrateLabel, new StandardPayloadDataCM(incomingMessageContents)));
                }
                else
                {
                    RequestPlanExecutionTermination("Incoming message doesn't belong to specified channel. No downstream activities are executed");
                }                
            }
            else
            {
                RequestPlanExecutionTermination("Plan successfully activated. It will wait and respond to specified Slack postings");
            }

            return Task.FromResult(0);
        }

        private List<KeyValueDTO> ExtractIncomingMessageContentFromPayload()
        {
            var eventReport = Payload.CrateContentsOfType<EventReportCM>().FirstOrDefault();

            return eventReport?.EventPayload.CrateContentsOfType<StandardPayloadDataCM>().SelectMany(x => x.AllValues()).ToList();
        }

        public override async Task Initialize()
        {
            var oAuthToken = AuthorizationToken.Token;
            ActivityUI.ChannelList.ListItems = (await _slackIntegration.GetChannelList(oAuthToken, false))
                .OrderBy(x => x.Key)
                .Select(x => new ListItem { Key = $"#{x.Key}", Value = x.Value })
                .ToList();

            EventSubscriptions.Manufacturer = "Slack";
            EventSubscriptions.Add("Slack Outgoing Message");

            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(ResultPayloadCrateLabel)
                 .AddField("token")
                .AddField("team_id")
                .AddField("team_domain")
                .AddField("service_id")
                .AddField("timestamp")
                .AddField("channel_id")
                .AddField("channel_name")
                .AddField("user_id")
                .AddField("user_name")
                .AddField("text");
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

    }
}