using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

    public class Publish_To_Slack_v1 : BaseTerminalActivity
    {
        private readonly ISlackIntegration _slackIntegration;

        public Publish_To_Slack_v1()
        {
            _slackIntegration = new SlackIntegration();
        }

        public async Task<PayloadDTO> Run(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(activityDO, containerId);
            string message; 

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            var actionChannelId = ExtractControlFieldValue(activityDO, "Selected_Slack_Channel");
            if (string.IsNullOrEmpty(actionChannelId))
            {
                return Error(payloadCrates, "No selected channelId found in activity.");
            }

            var payloadCrateStorage = CrateManager.GetStorage(payloadCrates);
            var configurationControls = GetConfigurationControls(activityDO);
            var messageField = (TextSource)GetControl(configurationControls, "Select_Message_Field", ControlTypes.TextSource);
            try
            {
                message = messageField.GetValue(payloadCrateStorage);
            }
            catch (ApplicationException ex)
            {
                return Error(payloadCrates, "Cannot get selected field value from TextSource control in activity. Detailed information: " + ex.Message);
            }


            await _slackIntegration.PostMessageToChat(authTokenDO.Token,
                actionChannelId, StripHTML(message));

            return Success(payloadCrates);
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        private List<FieldDTO> ExtractPayloadFields(PayloadDTO payloadCrates)
        {
            var payloadDataCrates = CrateManager.FromDto(payloadCrates.CrateStorage).CratesOfType<StandardPayloadDataCM>();

            var result = new List<FieldDTO>();
            foreach (var payloadDataCrate in payloadDataCrates)
            {
                result.AddRange(payloadDataCrate.Content.AllValues());
            }

            return result;
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
            var oauthToken = authTokenDO.Token;
            var configurationCrate = PackCrate_ConfigurationControls();
           await FillSlackChannelsSource(configurationCrate, "Selected_Slack_Channel", oauthToken);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(configurationCrate);             
                //crateStorage.Add(await CreateAvailableFieldsCrate(curActivityDO, "Available Fields"));
            }
            return curActivityDO;
        }

        protected async override Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            //{
            //    crateStorage.ReplaceByLabel(await CreateAvailableFieldsCrate(curActivityDO, "Available Fields"));
            //}

            return await Task.FromResult(curActivityDO);
        }

        private Crate PackCrate_ConfigurationControls()
        {
            var fieldSelectChannel = new DropDownList()
            {
                Label = "Select Slack Channel",
                Name = "Selected_Slack_Channel",
                Required = true,
                Source = null
            };

            var fieldSelect = CreateSpecificOrUpstreamValueChooser(
                "Select Message Field",
                "Select_Message_Field",
                addRequestConfigEvent: true,
                requestUpstream: true
            );

            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                fieldSelectChannel,
                fieldSelect
            };

            return CrateManager.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }

        private async Task<Crate> CreateAvailableFieldsCrate(ActivityDO activityDO)
        {
            var curUpstreamFields = (await GetCratesByDirection<FieldDescriptionsCM>(activityDO, CrateDirection.Upstream))
                .Where(x => x.Label != "Available Channels")
                .SelectMany(x => x.Content.Fields)
                .ToArray();

            var availableFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate(
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

        #region Fill Source
        private async Task FillSlackChannelsSource(Crate configurationCrate, string controlName, string oAuthToken)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var control = configurationControl.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                control.ListItems = await GetAllChannelList(oAuthToken);
            }
        }
        private async Task<List<ListItem>> GetAllChannelList(string oAuthToken)
        {
            var channels = await _slackIntegration.GetAllChannelList(oAuthToken);
            return channels.Select(x => new ListItem() { Key = x.Key, Value = x.Value }).ToList();
        }
        #endregion
    }
}