using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using terminalSlack.Interfaces;
using terminalSlack.Services;
using TerminalBase.BaseClasses;
using Fr8Data.States;

namespace terminalSlack.Actions
{

    public class Publish_To_Slack_v1 : BaseTerminalActivity
    {
        private readonly ISlackIntegration _slackIntegration;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Publish_To_Slack",
            Label = "Publish To Slack",
            Tags = "Notifier",
            Category = ActivityCategory.Forwarders,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            Version = "1",
            WebService = TerminalData.WebServiceDTO,
            MinPaneWidth = 330
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
                

        public override async Task Run()
        {
            string message;

            if (IsAuthenticationRequired)
            {
                RaiseNeedsAuthenticationError();
            }

            var actionChannelId = GetControl<DropDownList>("Selected_Slack_Channel")?.Value;
            if (string.IsNullOrEmpty(actionChannelId))
            {
                RaiseError("No selected channelId found in activity.");
            }

            var messageField = GetControl<TextSource>("Select_Message_Field", ControlTypes.TextSource);
            try
            {
                message = messageField.GetValue(Payload);
            }
            catch (ApplicationException ex)
            {
                RaiseError("Cannot get selected field value from TextSource control in activity. Detailed information: " + ex.Message);
            }

            try
            {
                await _slackIntegration.PostMessageToChat(AuthorizationToken.Token,
                    actionChannelId, StripHTML(messageField.GetValue(Payload)));
            }
            catch (TerminalBase.Errors.AuthorizationTokenExpiredOrInvalidException)
            {
                RaiseInvalidTokenError();
            }
            Success();
        }

        public override async Task Initialize()
        {
            var oauthToken = AuthorizationToken.Token;
            var configurationCrate = PackCrate_ConfigurationControls();
            await FillSlackChannelsSource(configurationCrate, "Selected_Slack_Channel", oauthToken);

            Storage.Clear();
            Storage.Add(configurationCrate);
        }

        public Publish_To_Slack_v1() :base(true)
        {
            _slackIntegration = new SlackIntegration();
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



        private Crate PackCrate_ConfigurationControls()
        {
            var fieldSelectChannel = new DropDownList()
            {
                Label = "Select Slack Channel",
                Name = "Selected_Slack_Channel",
                Required = true,
                Source = null
            };

            var fieldSelect = ControlHelper.CreateSpecificOrUpstreamValueChooser(
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

        private async Task<Crate> CreateAvailableFieldsCrate()
        {
            var curUpstreamFields = (await GetCratesByDirection<FieldDescriptionsCM>(CrateDirection.Upstream))
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

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

       


        #endregion
    }
}