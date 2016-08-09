using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using StructureMap;
using terminalSlack.Interfaces;

namespace terminalSlack.Activities
{
    public class Monitor_Channel_v2 : TerminalActivity<Monitor_Channel_v2.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("af0c038c-3adc-4372-b07e-e04b71102aa7"),
            Name = "Monitor_Channel",
            Label = "Monitor Slack Messages",
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            Version = "2",
            MinPaneWidth = 330,
            Categories = new[]
            {
                ActivityCategories.Monitor,
                TerminalData.ActivityCategoryDTO
            }
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


        private readonly ISlackIntegration _slackIntegration;
        private readonly ISlackEventManager _slackEventManager;

        public Monitor_Channel_v2(ICrateManager crateManager, ISlackIntegration slackIntegration, ISlackEventManager eventManager)
            : base(crateManager)
        {
            _slackIntegration = slackIntegration;
            _slackEventManager = eventManager;
        }

        public override async Task Initialize()
        {
            ActivityUI.ChannelList.ListItems = (await _slackIntegration.GetChannelList(AuthorizationToken.Token).ConfigureAwait(false))
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
            //No extra configuration is required
            return Task.FromResult(0);
        }

        protected override Task Validate()
        {
            if (!IsMonitoringDirectMessages && (!IsMonitoringChannels || (!IsMonitoringAllChannels && !IsMonitoringSpecificChannels)))
            {
                ValidationManager.SetError("At least one of the monitoring options must be selected", ActivityUI.MonitorDirectMessagesOption, ActivityUI.MonitorChannelsOption);
            }

            return Task.FromResult(0);
        }

        public override Task Run()
        {
            var incomingMessageContents = ExtractIncomingMessageContentFromPayload();
            var hasIncomingMessage = incomingMessageContents?.Count > 0;

            if (!hasIncomingMessage)
            {
                RequestPlanExecutionTermination("Incoming message is missing.");
                return Task.FromResult(0);
            }

            var incomingChannelId = incomingMessageContents.FirstOrDefault(x=>x.Key == "channel_id")?.Value;
            var isSentByCurrentUser = incomingMessageContents.FirstOrDefault(x => x.Key == "user_name")?.Value == AuthorizationToken.ExternalAccountId;

            if (string.IsNullOrEmpty(incomingChannelId))
            {
                RequestPlanExecutionTermination("Incoming message doesn't contain information about source channel");
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
                    Payload.Add(Crate.FromContent(ResultPayloadCrateLabel, new StandardPayloadDataCM(incomingMessageContents)));
                }
                else
                {
                    RequestPlanExecutionTermination("Incoming message doesn't pass filter criteria. No downstream activities are executed");
                }
            }

            return Task.FromResult(0);
        }

        public override async Task Activate()
        {
            await _slackEventManager.Subscribe(AuthorizationToken, ActivityPayload.RootPlanNodeId.Value).ConfigureAwait(false);
        }

        public override Task Deactivate()
        {
            _slackEventManager.Unsubscribe(ActivityPayload.RootPlanNodeId.Value);
            return Task.FromResult(0);
        }

        private List<KeyValueDTO> ExtractIncomingMessageContentFromPayload()
        {
            var eventReport = Payload.CrateContentsOfType<EventReportCM>().FirstOrDefault();
            if (eventReport == null)
            {
                return null;
            }
            return eventReport.EventPayload.CrateContentsOfType<StandardPayloadDataCM>().SelectMany(x => x.AllValues()).ToList();
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