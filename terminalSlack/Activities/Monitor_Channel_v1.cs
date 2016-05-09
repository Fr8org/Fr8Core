using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using terminalSlack.Interfaces;
using terminalSlack.Services;
using TerminalBase.BaseClasses;

namespace terminalSlack.Actions
{
    public class Monitor_Channel_v1 : EnhancedTerminalActivity<Monitor_Channel_v1.ActivityUi>
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

        public const string EventSubscriptionsCrateLabel = "Standard Event Subscriptions";

        private readonly ISlackIntegration _slackIntegration;

        public Monitor_Channel_v1() : base(true)
        {
            _slackIntegration = new SlackIntegration();
            ActivityName = "Monitor Channel";
        }

        protected override async Task Initialize(CrateSignaller crateSignaller)
        {
            var oAuthToken = AuthorizationToken.Token;
            ConfigurationControls.ChannelList.ListItems = (await _slackIntegration.GetChannelList(oAuthToken, false))
                .OrderBy(x => x.Key)
                .Select(x => new ListItem { Key = $"#{x.Key}", Value = x.Value })
                .ToList();
            CurrentActivityStorage.Add(CreateChannelPropertiesCrate());
            CurrentActivityStorage.Add(CreateEventSubscriptionCrate());
            crateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(ResultPayloadCrateLabel);
        }
        
        private Crate CreateChannelPropertiesCrate()
        {
            var fields = new[]
            {
                new FieldDTO() { Key = "token", Value = "token", Availability = AvailabilityType.Always },
                new FieldDTO() { Key = "team_id", Value = "team_id", Availability = AvailabilityType.Always },
                new FieldDTO() { Key = "team_domain", Value = "team_domain", Availability = AvailabilityType.Always },
                new FieldDTO() { Key = "service_id", Value = "service_id", Availability = AvailabilityType.Always },
                new FieldDTO() { Key = "timestamp", Value = "timestamp", Availability = AvailabilityType.Always },
                new FieldDTO() { Key = "channel_id", Value = "channel_id", Availability = AvailabilityType.Always },
                new FieldDTO() { Key = "channel_name", Value = "channel_name", Availability = AvailabilityType.Always },
                new FieldDTO() { Key = "user_id", Value = "user_id", Availability = AvailabilityType.Always },
                new FieldDTO() { Key = "user_name", Value = "user_name", Availability = AvailabilityType.Always },
                new FieldDTO() { Key = "text", Value = "text", Availability = AvailabilityType.Always }
            };
            var crate = Crate.FromContent(SlackMessagePropertiesCrateLabel, new FieldDescriptionsCM(fields), AvailabilityType.Always);
            return crate;
        }

        private Crate CreateEventSubscriptionCrate()
        {
            return CrateManager.CreateStandardEventSubscriptionsCrate(EventSubscriptionsCrateLabel, 
                                                                      "Slack", 
                                                                      new string[] { "Slack Outgoing Message" });
        }

        protected override Task Configure(CrateSignaller crateSignaller)
        {
            //No extra configuration is required
            return Task.FromResult(0);
        }

        protected override Task RunCurrentActivity()
        {
            var incomingMessageContents = ExtractIncomingMessageContentFromPayload();
            var hasIncomingMessage = incomingMessageContents?.Fields.Count > 0;
            if (hasIncomingMessage)
            {
                var channelMatches = ConfigurationControls.AllChannelsOption.Selected
                    || string.IsNullOrEmpty(ConfigurationControls.ChannelList.selectedKey)
                    || ConfigurationControls.ChannelList.Value == incomingMessageContents["channel_id"];
                if (channelMatches)
                {
                    CurrentPayloadStorage.Add(Crate.FromContent(ResultPayloadCrateLabel, new StandardPayloadDataCM(incomingMessageContents.Fields), AvailabilityType.RunTime));
                }
                else
                {
                    RequestHubExecutionTermination("Incoming message doesn't belong to specified channel. No downstream activities are executed");
                }                
            }
            else
            {
                RequestHubExecutionTermination("Plan successfully activated. It will wait and respond to specified Slack postings");
            }
            return Task.FromResult(0);
        }

        private FieldDescriptionsCM ExtractIncomingMessageContentFromPayload()
        {
            var eventReport = CurrentPayloadStorage.CrateContentsOfType<EventReportCM>().FirstOrDefault();
            if (eventReport == null)
            {
                return null;
            }
            return new FieldDescriptionsCM(eventReport.EventPayload.CrateContentsOfType<StandardPayloadDataCM>().SelectMany(x => x.AllValues()));
        }
    }
}