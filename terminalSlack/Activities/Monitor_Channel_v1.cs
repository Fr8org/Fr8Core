using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Hub.Managers;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using TerminalBase.Infrastructure;
using terminalSlack.Interfaces;
using terminalSlack.Services;
using TerminalBase.BaseClasses;
using Data.Entities;

namespace terminalSlack.Actions
{
    public class Monitor_Channel_v1 : BaseTerminalActivity
    {
        private readonly ISlackIntegration _slackIntegration;

        public Monitor_Channel_v1()
        {
            _slackIntegration = new SlackIntegration();
        }

        public async Task<PayloadDTO> Run(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(activityDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            List<FieldDTO> payloadFields;
            try
            {
                payloadFields = ExtractPayloadFields(payloadCrates);
            }
            catch (ArgumentException)
            {
                return ActivateAndTerminateExecution(activityDO, authTokenDO, payloadCrates).Result;
            }

            var payloadChannelIdField = payloadFields.FirstOrDefault(x => x.Key == "channel_id");
            if (payloadChannelIdField == null)
            {
                return await ActivateAndTerminateExecution(activityDO, authTokenDO, payloadCrates);
            }

            var payloadChannelId = payloadChannelIdField.Value;
            var actionChannelId = ExtractControlFieldValue(activityDO, "Selected_Slack_Channel");

            if (payloadChannelId != actionChannelId)
            {
                return Error(payloadCrates, "Unexpected channel-id.");
            }

            using (var crateStorage = CrateManager.GetUpdatableStorage(payloadCrates))
            {
                crateStorage.Add(Data.Crates.Crate.FromContent("Slack Payload Data", new StandardPayloadDataCM(payloadFields)));
            }

            return Success(payloadCrates);
        }

        private async Task<PayloadDTO> ActivateAndTerminateExecution(ActivityDO activityDO, AuthorizationTokenDO authTokenDO, PayloadDTO payloadCrates)
        {
            await Activate(activityDO, authTokenDO);
            return TerminateHubExecution(payloadCrates, "Plan successfully activated. It will wait and respond to specified Slack postings");
        }

        private List<FieldDTO> ExtractPayloadFields(PayloadDTO payloadCrates)
        {
            var eventReportMS = CrateManager.GetStorage(payloadCrates).CrateContentsOfType<EventReportCM>().SingleOrDefault();
            if (eventReportMS == null)
            {
                Error(payloadCrates, "EventReportCrate is empty.");
                throw new ArgumentException();
            }

            var eventFieldsCrate = eventReportMS.EventPayload.SingleOrDefault();
            if (eventFieldsCrate == null)
            {
                Error(payloadCrates, "EventReportMS.EventPayload is empty.");
                throw new ArgumentException();
            }

            return eventReportMS.EventPayload.CrateContentsOfType<StandardPayloadDataCM>().SelectMany(x => x.AllValues()).ToList();
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return curActivityDO;
            }

            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var oAuthToken = authTokenDO.Token;
            var configurationCrate = CreateControlsCrate();
            await FillSlackChannelsSource(configurationCrate, "Selected_Slack_Channel", oAuthToken);

            var crateDesignTimeFields = CreateDesignTimeFieldsCrate();
            var crateEventSubscriptions = CreateEventSubscriptionCrate();

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(configurationCrate);
                crateStorage.Add(crateDesignTimeFields);
                crateStorage.Add(crateEventSubscriptions);
            }
            return await Task.FromResult<ActivityDO>(curActivityDO);
        }

        private Crate CreateControlsCrate()
        {
            var selectedSlackChannel = new DropDownList()
                {
                    Label = "Select Slack Channel",
                    Name = "Selected_Slack_Channel",
                    Required = true,
                Source = null
            };

            var infoLabel = GenerateTextBlock("",
                    @"Slack doesn't currently offer a way for us to automatically request events for this channel. 
                    To enable events to be sent to Fr8, do the following: </br>
                    <ol>
                        <li>Go to https://{yourteamname}.slack.com/services/new/outgoing-webhook. </li>
                        <li>Click 'Add Outgoing WebHooks Integration'</li>
                        <li>In the Outgoing WebHook form go to 'URL(s)' field fill the following address: 
                            <strong>https://terminalslack.fr8.co/terminals/terminalslack/events</strong>
                        </li>
                    </ol>",
                    "", "Info_Label");

            return PackControlsCrate(selectedSlackChannel, infoLabel);
        }

        private Crate CreateDesignTimeFieldsCrate()
        {
            var fields = new List<FieldDTO>()
            {
                new FieldDTO() { Key = "token", Value = "token" },
                new FieldDTO() { Key = "team_id", Value = "team_id" },
                new FieldDTO() { Key = "team_domain", Value = "team_domain" },
                new FieldDTO() { Key = "service_id", Value = "service_id" },
                new FieldDTO() { Key = "timestamp", Value = "timestamp" },
                new FieldDTO() { Key = "channel_id", Value = "channel_id" },
                new FieldDTO() { Key = "channel_name", Value = "channel_name" },
                new FieldDTO() { Key = "user_id", Value = "user_id" },
                new FieldDTO() { Key = "user_name", Value = "user_name" },
                new FieldDTO() { Key = "text", Value = "text" }
            };

            var crate =
                CrateManager.CreateDesignTimeFieldsCrate(
                    "Available Fields",
                    fields.ToArray()
                );

            return crate;
        }

        private Crate CreateEventSubscriptionCrate()
        {
            var subscriptions = new string[] {
                "Slack Outgoing Message"
            };

            return CrateManager.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                "Slack",
                subscriptions.ToArray()
                );
        }

        #region Fill Source
        private async Task FillSlackChannelsSource(Crate configurationCrate, string controlName, string oAuthToken)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var control = configurationControl.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                control.ListItems = await GetChannelList(oAuthToken);
            }
        }
        private async Task<List<ListItem>> GetChannelList(string oAuthToken)
        {
            var channels = await _slackIntegration.GetChannelList(oAuthToken);
            return channels.Select(x => new ListItem() { Key = x.Key, Value = x.Value }).ToList();
        }
        #endregion
    }
}