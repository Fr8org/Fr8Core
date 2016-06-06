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
using Newtonsoft.Json;
using terminalYammer.Interfaces;
using terminalYammer.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalYammer.Actions
{
    public class Post_To_Yammer_v1 : BaseTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Post_To_Yammer",
            Label = "Post To Yammer",
            Tags = "Notifier",
            Category = ActivityCategory.Forwarders,
            NeedsAuthentication = true,
            Version = "1",
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            WebService = TerminalData.WebServiceDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private readonly IYammer _yammer;

        public class GroupMessage
        {
            public string GroupID;
            public string Message;
        }

        public class ActivityUi : StandardConfigurationControlsCM
        {

            [JsonIgnore]
            public DropDownList Groups { get; set; }

            [JsonIgnore]
            public TextSource Message { get; set; }

            public ActivityUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add((Groups = new DropDownList
               {
                   Label = "Select Yammer Group",
                   Name = "Groups",
                   Required = true,
                   Source = new FieldSourceDTO { Label = "Available Groups", ManifestType = CrateManifestTypes.StandardDesignTimeFields }
               }));

                Controls.Add((Message = new TextSource("Select Message Field", "Available Fields", "Message")
                {
                    Source = new FieldSourceDTO()
                    {
                        Label = "Available Fields",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                        RequestUpstream = true
                    }
                }));

                Message.Events.Add(new ControlEvent("onChange", "requestConfig"));
            }
        }

        public Post_To_Yammer_v1(ICrateManager crateManager)
            : base(crateManager)
        {
            _yammer = new Yammer();
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        public override async Task Initialize()
        {
            var oauthToken = AuthorizationToken.Token;
            var groups = await _yammer.GetGroupsList(oauthToken);
            var crateAvailableGroups = CreateAvailableGroupsCrate(groups);
            Storage.Clear();
            Storage.Add(PackControls(new ActivityUi()));
            Storage.Add(crateAvailableGroups);
        }

        public override async Task Run()
        {
            if (ConfigurationControls == null)
            {
                throw new ApplicationException("Action was not configured correctly");
            }
            var groupMessageField = GetGroupMessageFields(ConfigurationControls, Payload);
            ValidateYammerActivity(groupMessageField.GroupID, "No selected group found in activity.");
            ValidateYammerActivity(groupMessageField.Message, "No selected field found in activity.");
            try
            {
                await _yammer.PostMessageToGroup(AuthorizationToken.Token,
                    groupMessageField.GroupID, groupMessageField.Message);
            }
            catch (TerminalBase.Errors.AuthorizationTokenExpiredOrInvalidException)
            {
                RaiseInvalidTokenError();
                return;
            }
        }

        private Crate CreateAvailableGroupsCrate(IEnumerable<FieldDTO> groups)
        {
            var crate =
                CrateManager.CreateDesignTimeFieldsCrate(
                    "Available Groups",
                    groups.ToArray()
                );

            return crate;
        }

        private  GroupMessage GetGroupMessageFields(StandardConfigurationControlsCM ui, ICrateStorage payload)
        {
            var controls = new ActivityUi();
            controls.ClonePropertiesFrom(ui);

            var groupMessage = new GroupMessage();
            groupMessage.GroupID = controls.Groups.Value;

            //Quick fix FR-2719
            var messageField = GetControl<TextSource>("Message");
            groupMessage.Message = messageField.GetValue(payload);
            return groupMessage;
        }

       

        private void ValidateYammerActivity(string value, string exceptionMessage)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ApplicationException(exceptionMessage);
            }
        }
    }
}