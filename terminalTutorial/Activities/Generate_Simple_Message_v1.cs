using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using Fr8Data.Managers;

namespace terminalTutorial.Actions
{
    public class Generate_Simple_Message_v1 : BaseTerminalActivity
    {
        public Generate_Simple_Message_v1(ICrateManager crateManager) : base(false, crateManager)
        {
        }

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Generate_Simple_Message",
            Label = "Generate Simple Message",
            Category = ActivityCategory.Processors,
            NeedsAuthentication = false,
            Version = "1",
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            WebService = null
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

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


        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        public override async Task Initialize()
        {
            Storage.Add(PackControls(new ActivityUi()));
        }

        public override async Task Run()
        {
            //if (ConfigurationControls == null)
            //{
            //    throw new ApplicationException("Action was not configured correctly");
            //}
            //var groupMessageField = GetGroupMessageFields(ConfigurationControls, Payload);
            //ValidateYammerActivity(groupMessageField.GroupID, "No selected group found in activity.");
            //ValidateYammerActivity(groupMessageField.Message, "No selected field found in activity.");
            //try
            //{
            //    await _yammer.PostMessageToGroup(AuthorizationToken.Token,
            //        groupMessageField.GroupID, groupMessageField.Message);
            //}
            //catch (TerminalBase.Errors.AuthorizationTokenExpiredOrInvalidException)
            //{
            //    RaiseInvalidTokenError();
            //    return;
            //}
        }
    }
}