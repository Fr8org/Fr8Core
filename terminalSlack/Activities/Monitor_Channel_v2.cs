using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using StructureMap;
using terminalSlack.Interfaces;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalSlack.Activities
{
    public class Monitor_Channel_v2 : EnhancedTerminalActivity<Monitor_Channel_v2.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Monitor_Channel",
            Label = "Monitor Slack Messages",
            Category = ActivityCategory.Monitors,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            Version = "2",
            WebService = TerminalData.WebServiceDTO,
            MinPaneWidth = 330
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public CheckBox MonitorDirectMessagesOption { get; set; }

            public CheckBox MonitorChannelsOption { get; set; }

            public RadioButtonGroup ChannelSelectionGroup { get; set; }

            public RadioButtonOption AllChannelsOption { get; set; }

            public RadioButtonOption SpecificChannelOption { get; set; }

            public DropDownList ChannelList { get; set; }

            public ActivityUi()
            {
                MonitorDirectMessagesOption = new CheckBox
                {
                    Label = "Monitor my direct messages and group conversations",
                    Name = nameof(MonitorDirectMessagesOption)
                };
                MonitorChannelsOption = new CheckBox
                {
                    Label = "Monitor channels",
                    Name = nameof(MonitorChannelsOption),
                    Selected = true
                };
                AllChannelsOption = new RadioButtonOption
                {
                    Value = "All",
                    Name = nameof(AllChannelsOption),
                    Selected = true
                };
                ChannelList = new DropDownList();
                SpecificChannelOption = new RadioButtonOption
                {
                    Controls = new List<ControlDefinitionDTO> { ChannelList },
                    Name = nameof(SpecificChannelOption),
                    Value = "Select channel"
                };
                ChannelSelectionGroup = new RadioButtonGroup
                {
                    GroupName = nameof(ChannelSelectionGroup),
                    Name = nameof(ChannelSelectionGroup),
                    Radios = new List<RadioButtonOption> { AllChannelsOption, SpecificChannelOption },
                    Label = "Monitor which channels?"
                };
                Controls.Add(MonitorDirectMessagesOption);
                Controls.Add(MonitorChannelsOption);
                Controls.Add(ChannelSelectionGroup);
            }
        }

        public const string ResultPayloadCrateLabel = "Slack Message";

        public const string EventSubscriptionsCrateLabel = "Standard Event Subscriptions";

        private readonly ISlackIntegration _slackIntegration;
        private readonly ISlackEventManager _slackEventManager;

        public Monitor_Channel_v2(ICrateManager crateManager)
            : base(true, crateManager)
        {
            _slackIntegration = ObjectFactory.GetInstance<ISlackIntegration>();
            _slackEventManager = ObjectFactory.GetInstance<ISlackEventManager>();
        }

        protected override async Task InitializeETA()
        {
            ActivityUI.ChannelList.ListItems = (await _slackIntegration.GetChannelList(AuthorizationToken.Token).ConfigureAwait(false))
                .OrderBy(x => x.Key)
                .Select(x => new ListItem { Key = $"#{x.Key}", Value = x.Value })
                .ToList();
            Storage.Add(CreateEventSubscriptionCrate());
            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(ResultPayloadCrateLabel)
                               .AddFields(GetChannelProperties());
        }

        private IEnumerable<FieldDTO> GetChannelProperties()
        {
            yield return new FieldDTO { Key = "team_id", Value = "team_id", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "team_domain", Value = "team_domain", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "timestamp", Value = "timestamp", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "channel_id", Value = "channel_id", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "channel_name", Value = "channel_name", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "user_id", Value = "user_id", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "user_name", Value = "user_name", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "text", Value = "text", Availability = AvailabilityType.Always };
        }

        private Crate CreateEventSubscriptionCrate()
        {
            return CrateManager.CreateStandardEventSubscriptionsCrate(EventSubscriptionsCrateLabel, "Slack", "Slack Outgoing Message");
        }

        protected override Task ConfigureETA()
        {
            //No extra configuration is required
            return Task.FromResult(0);
        }

        protected override Task ValidateETA()
        {
            if (!IsMonitoringDirectMessages && (!IsMonitoringChannels || (!IsMonitoringAllChannels && !IsMonitoringSpecificChannels)))
            {
                ValidationManager.SetError("At least one of the monitoring options must be selected", ActivityUI.MonitorDirectMessagesOption, ActivityUI.MonitorChannelsOption);
            }

            return Task.FromResult(0);
        }

        protected override Task RunETA()
        {
            var incomingMessageContents = ExtractIncomingMessageContentFromPayload();
            var hasIncomingMessage = incomingMessageContents?.Fields?.Count > 0;

            if (!hasIncomingMessage)
            {
                TerminateHubExecution("Incoming message is missing.");
                return Task.FromResult(0);
            }

            var incomingChannelId = incomingMessageContents["channel_id"];
            var isSentByCurrentUser = incomingMessageContents["user_name"] == AuthorizationToken.ExternalAccountId;

            if (string.IsNullOrEmpty(incomingChannelId))
            {
                TerminateHubExecution("Incoming message doesn't contain information about source channel");
            }
            else
            {
                //Slack channel Id first letter: C - for channel, D - for direct messages to current user, G - for message in group conversation with current user
                var isSentToChannel = incomingChannelId.StartsWith("C", StringComparison.OrdinalIgnoreCase);
                var isDirect = incomingChannelId.StartsWith("D", StringComparison.OrdinalIgnoreCase);
                var isSentToGroup = incomingChannelId.StartsWith("G", StringComparison.OrdinalIgnoreCase);
                //Message is sent to tracked channel or is sent directly or to group but not by current user
                var isTrackedByChannel = IsMonitoringChannels && isSentToChannel && (IsMonitoringAllChannels || incomingChannelId == SelectedChannelId);
                var isTrackedByUser = IsMonitoringDirectMessages && (isDirect || isSentToGroup) && !isSentByCurrentUser;
                if (isTrackedByUser || isTrackedByChannel)
                {
                    Payload.Add(Crate.FromContent(ResultPayloadCrateLabel, new StandardPayloadDataCM(incomingMessageContents.Fields), AvailabilityType.RunTime));
                }
                else
                {
                    TerminateHubExecution("Incoming message doesn't pass filter criteria. No downstream activities are executed");
                }
            }

            return Task.FromResult(0);
        }

        protected override async Task ActivateETA()
        {
            await _slackEventManager.Subscribe(AuthorizationToken, ActivityPayload.RootPlanNodeId.Value).ConfigureAwait(false);
        }

        protected override Task DeactivateETA()
        {
            _slackEventManager.Unsubscribe(ActivityPayload.RootPlanNodeId.Value);
            return Task.FromResult(0);
        }

        private FieldDescriptionsCM ExtractIncomingMessageContentFromPayload()
        {
            var eventReport = Payload.CrateContentsOfType<EventReportCM>().FirstOrDefault();
            if (eventReport == null)
            {
                return null;
            }
            return new FieldDescriptionsCM(eventReport.EventPayload.CrateContentsOfType<StandardPayloadDataCM>().SelectMany(x => x.AllValues()));
        }

        #region Control properties wrappers

        private bool IsMonitoringChannels => ActivityUI.MonitorChannelsOption.Selected;

        private bool IsMonitoringDirectMessages => ActivityUI.MonitorDirectMessagesOption.Selected;

        private bool IsMonitoringAllChannels => ActivityUI.AllChannelsOption.Selected
                                            || (ActivityUI.SpecificChannelOption.Selected && string.IsNullOrEmpty(ActivityUI.ChannelList.Value));

        private bool IsMonitoringSpecificChannels => ActivityUI.SpecificChannelOption.Selected;

        private string SelectedChannelId => ActivityUI.ChannelList.Value;

        #endregion
    }
}