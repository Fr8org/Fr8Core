using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using TerminalBase.Infrastructure;
using terminalSlack.Interfaces;
using terminalSlack.Services;
using TerminalBase.BaseClasses;
using Data.Entities;
using Data.States;

namespace terminalSlack.Actions
{

    public class Publish_To_Slack_v1 : BaseTerminalAction
    {
        private readonly ISlackIntegration _slackIntegration;

        public Publish_To_Slack_v1()
        {
            _slackIntegration = new SlackIntegration();
        }

        public async Task<PayloadDTO> Run(ActionDO actionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(actionDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            var actionChannelId = ExtractControlFieldValue(actionDO, "Selected_Slack_Channel");
            if (string.IsNullOrEmpty(actionChannelId))
            {
                return Error(payloadCrates, "No selected channelId found in action.");
            }

            var actionFieldName = ExtractControlFieldValue(actionDO, "Select_Message_Field");
            if (string.IsNullOrEmpty(actionFieldName))
            {
                return Error(payloadCrates, "No selected field found in action.");
            }

            var payloadFields = ExtractPayloadFields(payloadCrates);

            var payloadMessageField = payloadFields.FirstOrDefault(x => x.Key == actionFieldName);
            if (payloadMessageField == null)
            {
                return Error(payloadCrates, "No specified field found in action.");
            }

            await _slackIntegration.PostMessageToChat(authTokenDO.Token,
                actionChannelId, payloadMessageField.Value);

            return Success(payloadCrates);
        }

        private List<FieldDTO> ExtractPayloadFields(PayloadDTO payloadCrates)
        {
            var payloadDataCrates = Crate.FromDto(payloadCrates.CrateStorage).CratesOfType<StandardPayloadDataCM>();

            var result = new List<FieldDTO>();
            foreach (var payloadDataCrate in payloadDataCrates)
            {
                result.AddRange(payloadDataCrate.Content.AllValues());
            }

            return result;
        }

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            return Crate.IsStorageEmpty(curActionDO) ? ConfigurationRequestType.Initial : ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var oauthToken = authTokenDO.Token;
            var channels = await _slackIntegration.GetChannelList(oauthToken);

            var crateControls = PackCrate_ConfigurationControls();
            var crateAvailableChannels = CreateAvailableChannelsCrate(channels);
            var crateAvailableFields = await CreateAvailableFieldsCrate(curActionDO);

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(crateControls);
                updater.CrateStorage.Add(crateAvailableChannels);
                updater.CrateStorage.Add(crateAvailableFields);
            }

            return curActionDO;
        }

        protected async override Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.ReplaceByLabel(await CreateAvailableFieldsCrate(curActionDO));
            }

            return curActionDO;
        }

        private Crate PackCrate_ConfigurationControls()
        {
            var fieldSelectChannel = new DropDownList()
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
            };

            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                fieldSelectChannel,
                new TextSource("Select Message Field", "Available Fields", "Select_Message_Field")
            };

            return Crate.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
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

        private async Task<Crate> CreateAvailableFieldsCrate(ActionDO actionDO)
        {
            var curUpstreamFields =
                (await GetCratesByDirection<StandardDesignTimeFieldsCM>(actionDO, CrateDirection.Upstream))
                .Where(x => x.Label != "Available Channels")
                .SelectMany(x => x.Content.Fields)
                .ToArray();

            var availableFieldsCrate = Crate.CreateDesignTimeFieldsCrate(
                    "Available Fields",
                    curUpstreamFields
                );

            return availableFieldsCrate;
        }

        // TODO: finish that later.
        /*
        public object Execute(SlackPayloadDTO curSlackPayload)
        {
            string responseText = string.Empty;
            Encoding encoding = new UTF8Encoding();

            const string webhookUrl = "WebhookUrl";
            Uri uri = new Uri(ConfigurationManager.AppSettings[webhookUrl]);

            string payloadJson = JsonConvert.SerializeObject(curSlackPayload);

            using (WebClient client = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                data["payload"] = payloadJson;

                var response = client.UploadValues(uri, "POST", data);

                responseText = encoding.GetString(response);
            }
            return responseText;
        }
        */
    }
}