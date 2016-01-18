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
            string message; 

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            var actionChannelId = ExtractControlFieldValue(actionDO, "Selected_Slack_Channel");
            if (string.IsNullOrEmpty(actionChannelId))
            {
                return Error(payloadCrates, "No selected channelId found in action.");
            }

            var payloadCrateStorage = Crate.GetStorage(payloadCrates);
            var configurationControls = GetConfigurationControls(actionDO);
            var messageField = (TextSource)GetControl(configurationControls, "Select_Message_Field", ControlTypes.TextSource);
            try
            {
                message = messageField.GetValue(payloadCrateStorage);
            }
            catch (ApplicationException ex)
            {
                return Error(payloadCrates, "Cannot get selected filed value from TextSource control in action. Detailed information: " + ex.Message);
            }


            await _slackIntegration.PostMessageToChat(authTokenDO.Token,
                actionChannelId, message);

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
            var channels = await _slackIntegration.GetAllChannelList(oauthToken);

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackCrate_ConfigurationControls());
                updater.CrateStorage.Add(CreateAvailableChannelsCrate(channels));
                updater.CrateStorage.Add(await CreateAvailableFieldsCrate(curActionDO, "Available Fields"));
            }

            return curActionDO;
        }

        protected async override Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.ReplaceByLabel(await CreateAvailableFieldsCrate(curActionDO, "Available Fields"));
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
                Source = new FieldSourceDTO
                {
                    Label = "Available Channels",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            var fieldSelect = new TextSource("Select Message Field", "Available Fields", "Select_Message_Field");
            fieldSelect.Events.Add(new ControlEvent("onChange", "requestConfig"));
            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                fieldSelectChannel,
                fieldSelect
            };

            return Crate.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }

        private Crate CreateAvailableChannelsCrate(IEnumerable<FieldDTO> channels)
        {
            var crate =
                Crate.CreateDesignTimeFieldsCrate(
                    "Available Channels",
                    AvailabilityType.Configuration,
                    channels.ToArray()
                );

            return crate;
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