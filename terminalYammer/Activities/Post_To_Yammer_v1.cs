using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using terminalYammer.Interfaces;
using terminalYammer.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalYammer.Activities
{

    public class Post_To_Yammer_v1 : BaseTerminalActivity
    {
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

        public Post_To_Yammer_v1()
        {
            _yammer = new Yammer();
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
            var groups = await _yammer.GetGroupsList(oauthToken);

            var crateAvailableGroups = CreateAvailableGroupsCrate(groups);
            var crateAvailableFields = await CreateAvailableFieldsCrate(curActivityDO);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(PackControls(new ActivityUi()));
                crateStorage.Add(crateAvailableGroups);
                crateStorage.Add(crateAvailableFields);
            }

            return curActivityDO;
        }

        public async Task<PayloadDTO> Run(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var ui = CrateManager.GetStorage(activityDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();
            var processPayload = await GetPayload(activityDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(processPayload);
            }

            if (ui == null)
            {
                throw new ApplicationException("Action was not configured correctly");
            }
            var groupMessageField = GetGroupMessageFields(ui, CrateManager.GetStorage(processPayload));

            ValidateYammerActivity(groupMessageField.GroupID, "No selected group found in activity.");
            ValidateYammerActivity(groupMessageField.Message, "No selected field found in activity.");

            await _yammer.PostMessageToGroup(authTokenDO.Token,
                groupMessageField.GroupID, groupMessageField.Message);

            return processPayload;
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
            var messageField = (TextSource)GetControl(ui, "Message", ControlTypes.TextSource);
            groupMessage.Message = messageField.GetValue(payload);

            return groupMessage;
        }

        private async Task<Crate> CreateAvailableFieldsCrate(ActivityDO activityDO)
        {
            var curUpstreamFields =
                (await GetCratesByDirection<FieldDescriptionsCM>(activityDO, CrateDirection.Upstream))

                .Where(x => x.Label != "Available Groups")
                .SelectMany(x => x.Content.Fields)
                .ToArray();

            var availableFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate(
                    "Available Fields",
                    curUpstreamFields
                );

            return availableFieldsCrate;
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