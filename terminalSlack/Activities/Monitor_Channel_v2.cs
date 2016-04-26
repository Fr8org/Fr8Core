using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using terminalSlack.Interfaces;
using terminalSlack.Services;
using TerminalBase.BaseClasses;

namespace terminalSlack.Actions
{
    public class Monitor_Channel_v2 : EnhancedTerminalActivity<Monitor_Channel_v2.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public RadioButtonGroup ChannelSelectionGroup { get; set; }

            public RadioButtonOption AllChannelsOption { get; set; }

            public RadioButtonOption SpecificChannelOption { get; set; }

            public DropDownList ChannelList { get; set; }

            public CheckBox IncludeDirectMessagesOption { get; set; }

            public ActivityUi()
            {
                AllChannelsOption = new RadioButtonOption
                {
                    Name = nameof(AllChannelsOption),
                    Value = "All",
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
                    Value = "Select channel"
                };
                ChannelSelectionGroup = new RadioButtonGroup
                {
                    Name = nameof(ChannelSelectionGroup),
                    GroupName = nameof(ChannelSelectionGroup),
                    Radios = new List<RadioButtonOption> { AllChannelsOption, SpecificChannelOption },
                    Label = "Monitor which channels?"
                };
                IncludeDirectMessagesOption = new CheckBox
                {
                    Name = nameof(IncludeDirectMessagesOption),
                    Label = "Include direct messages to me"
                };
                Controls.Add(ChannelSelectionGroup);
                Controls.Add(IncludeDirectMessagesOption);
            }
        }

        public const string ResultPayloadCrateLabel = "Slack Message";

        public const string EventSubscriptionsCrateLabel = "Standard Event Subscriptions";

        private readonly ISlackIntegration _slackIntegration;

        private readonly 

        public Monitor_Channel_v2() : base(true)
        {
            _slackIntegration = new SlackIntegration();
            ActivityName = "Monitor Channel";
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            ConfigurationControls.ChannelList.ListItems = (await _slackIntegration.GetChannelList(AuthorizationToken.Token))
                .OrderBy(x => x.Key)
                .Select(x => new ListItem { Key = $"#{x.Key}", Value = x.Value })
                .ToList();
            CurrentActivityStorage.Add(CreateEventSubscriptionCrate());
            runtimeCrateManager.MarkAvailableAtRuntime<StandardPayloadDataCM>(ResultPayloadCrateLabel)
                               .AddFields(GetChannelProperties());
        }

        private IEnumerable<FieldDTO> GetChannelProperties()
        {
            yield return new FieldDTO { Key = "token", Value = "token", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "team_id", Value = "team_id", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "team_domain", Value = "team_domain", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "service_id", Value = "service_id", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "timestamp", Value = "timestamp", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "channel_id", Value = "channel_id", Availability = AvailabilityType.Always },
            yield return new FieldDTO { Key = "channel_name", Value = "channel_name", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "user_id", Value = "user_id", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "user_name", Value = "user_name", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "text", Value = "text", Availability = AvailabilityType.Always };
        }

        private Crate CreateEventSubscriptionCrate()
        {
            return CrateManager.CreateStandardEventSubscriptionsCrate(EventSubscriptionsCrateLabel, "Slack", "Slack Outgoing Message");
        }

        protected override Task Configure(RuntimeCrateManager runtimeCrateManager)
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

        protected override Task Deactivate()
        {
            //TODO: remove subscription
            return base.Deactivate();
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