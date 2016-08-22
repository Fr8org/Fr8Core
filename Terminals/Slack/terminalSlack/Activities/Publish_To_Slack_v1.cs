using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Infrastructure;
using terminalSlack.Interfaces;

namespace terminalSlack.Activities
{

    public class Publish_To_Slack_v1 : ExplicitTerminalActivity
    {
        private readonly ISlackIntegration _slackIntegration;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("4698C675-CA2C-4BE7-82F9-2421F3608E13"),
            Name = "Publish_To_Slack",
            Label = "Publish To Slack",
            Tags = "Notifier",
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            Version = "1",
            MinPaneWidth = 330,
            Categories = new[]
            {
                ActivityCategories.Forward,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;


        protected override Task Validate()
        {
            var messageField = GetControl<TextSource>("Select_Message_Field");
            var actionChannelId = GetControl<DropDownList>("Selected_Slack_Channel").Value;

            if (string.IsNullOrEmpty(actionChannelId))
            {
                ValidationManager.SetError("Channel or user is not specified", "Selected_Slack_Channel");
            }

            ValidationManager.ValidateTextSourceNotEmpty(messageField, "Can't post empty message to Slack");

            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            var actionChannelId = GetControl<DropDownList>("Selected_Slack_Channel")?.Value;

            if (string.IsNullOrEmpty(actionChannelId))
            {
                RaiseError("No selected channelId found in activity.");
            }

            var messageField = GetControl<TextSource>("Select_Message_Field");
            
            try
            {
                await _slackIntegration.PostMessageToChat(AuthorizationToken.Token,
                    actionChannelId, StripHTML(messageField.TextValue));
            }
            catch (AuthorizationTokenExpiredOrInvalidException)
            {
                RaiseInvalidTokenError();
            }
            Success();
        }

        public override async Task Initialize()
        {
            Storage.Clear();

            var oauthToken = AuthorizationToken.Token;
            PackCrate_ConfigurationControls();

            await FillSlackChannelsSource("Selected_Slack_Channel", oauthToken);
        }

        public Publish_To_Slack_v1(ICrateManager crateManager, ISlackIntegration slackIntegration)
            : base(crateManager)
        {
            _slackIntegration = slackIntegration;
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        private void PackCrate_ConfigurationControls()
        {
            var fieldSelectChannel = new DropDownList()
            {
                Label = "Select Slack Channel",
                Name = "Selected_Slack_Channel",
                Required = true,
                Source = null
            };

            var fieldSelect = UiBuilder.CreateSpecificOrUpstreamValueChooser(
                "Select Message Field",
                "Select_Message_Field",
                addRequestConfigEvent: true,
                requestUpstream: true
            );

            AddControls(fieldSelectChannel, fieldSelect);
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
        private async Task FillSlackChannelsSource(string controlName, string oAuthToken)
        {
            var control = ConfigurationControls.FindByNameNested<DropDownList>(controlName);
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