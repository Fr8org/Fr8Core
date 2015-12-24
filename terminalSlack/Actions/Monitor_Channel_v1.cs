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
    public class Monitor_Channel_v1 : BaseTerminalAction
    {
        private readonly ISlackIntegration _slackIntegration;

        public Monitor_Channel_v1()
        {
            _slackIntegration = new SlackIntegration();
        }

        public async Task<PayloadDTO> Run(ActionDO actionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var processPayload = await GetProcessPayload(actionDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(processPayload);
            }


            List<FieldDTO> payloadFields;
            try
            {
                payloadFields = ExtractPayloadFields(processPayload);
            }
            catch (ArgumentException)
            {
                return processPayload;
            }
            

            var payloadChannelIdField = payloadFields.FirstOrDefault(x => x.Key == "channel_id");
            if (payloadChannelIdField == null)
            {
                return Error(processPayload, "No channel_id field found in payload.");
            }

            var payloadChannelId = payloadChannelIdField.Value;
            var actionChannelId = ExtractControlFieldValue(actionDO, "Selected_Slack_Channel");

            if (payloadChannelId != actionChannelId)
            {
                return Error(processPayload, "Unexpected channel-id.");
            }

            using (var updater = Crate.UpdateStorage(processPayload))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Slack Payload Data", new StandardPayloadDataCM(payloadFields)));
            }

            return Success(processPayload);
        }

        private List<FieldDTO> ExtractPayloadFields(PayloadDTO processPayload)
        {
            var eventReportMS = Crate.GetStorage(processPayload).CrateContentsOfType<EventReportCM>().SingleOrDefault();
            if (eventReportMS == null)
            {
                Error(processPayload, "EventReportCrate is empty.");
                throw new ArgumentException();
            }

            var eventFieldsCrate = eventReportMS.EventPayload.SingleOrDefault();
            if (eventFieldsCrate == null)
            {
                Error(processPayload, "EventReportMS.EventPayload is empty.");
                throw new ArgumentException();
            }

            return eventReportMS.EventPayload.CrateContentsOfType<StandardPayloadDataCM>().SelectMany(x => x.AllValues()).ToList();
        }

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator,authTokenDO);
        }

        private ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var oauthToken = authTokenDO.Token;
            var channels = await _slackIntegration.GetChannelList(oauthToken);

            var crateDesignTimeFields = CreateDesignTimeFieldsCrate();
            var crateAvailableChannels = CreateAvailableChannelsCrate(channels);
            var crateEventSubscriptions = CreateEventSubscriptionCrate();

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                PackConfigurationControls(updater.CrateStorage);
                updater.CrateStorage.Add(crateDesignTimeFields);
                updater.CrateStorage.Add(crateAvailableChannels);
                updater.CrateStorage.Add(crateEventSubscriptions);
            }


            return await Task.FromResult<ActionDO>(curActionDO);
        }

        private void PackConfigurationControls(CrateStorage crateStorage)
        {
            AddControl(
                crateStorage,
                new DropDownList()
                {
                    Label = "Select Slack Channel",
                    Name = "Selected_Slack_Channel",
                    Required = true,
                    Events = new List<ControlEvent>()
                    {
                        new ControlEvent("onChange", "requestConfig")
                    },
                    Source = new FieldSourceDTO
                    {
                        Label = "Available Channels",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    }
                });

            AddControl(
                crateStorage,
                GenerateTextBlock("Info_Label",
                    "Slack doesn't currently offer a way for us to automatically request events for this channel. You can do it manually here. use the following values: URL: <strong>http://www.fr8.company/events?dockyard_plugin=terminalSlack&version=1.0</strong>",
                    "", "Info_Label"));
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
                Crate.CreateDesignTimeFieldsCrate(
                    "Available Fields",
                    fields.ToArray()
                );

            return crate;
        }

        private Crate CreateAvailableChannelsCrate(IEnumerable<FieldDTO> channels)
        {
            var crate =
                Crate.CreateDesignTimeFieldsCrate(
                    "Available Channels",
                    channels.ToArray()
                );

            return crate;
        }

        private Crate CreateEventSubscriptionCrate()
        {
            var subscriptions = new string[] {
                "Slack Outgoing Message"
            };

            return Crate.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                subscriptions.ToArray()
                );
        }
    }
}