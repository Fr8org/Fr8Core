using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using TerminalBase.Infrastructure;
using terminalYammer.Interfaces;
using terminalYammer.Services;
using TerminalBase.BaseClasses;
using Data.Entities;
using Data.States;
using Newtonsoft.Json;

namespace terminalYammer.Actions
{

    public class Post_To_Yammer_v1 : BaseTerminalActivity
    {
        private readonly IYammer _yammer;

        public class GroupMessage
        {
            public string GroupID;
            public string Message;
        }

        public class ActionUi : StandardConfigurationControlsCM
        {

            [JsonIgnore]
            public DropDownList Groups { get; set; }

            [JsonIgnore]
            public TextSource Message { get; set; }

            public ActionUi()
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
                ));
            }
        }

        public Post_To_Yammer_v1()
        {
            _yammer = new Yammer();
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
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
                crateStorage.Add(PackControls(new ActionUi()));
                crateStorage.Add(crateAvailableGroups);
                crateStorage.Add(crateAvailableFields);
            }

            return curActivityDO;
        }

        public async Task<PayloadDTO> Run(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {

            CheckAuthentication(authTokenDO);
            var ui = CrateManager.GetStorage(activityDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            if (ui == null)
            {
                throw new ApplicationException("Action was not configured correctly");
            }
            var groupMessageField = GetGroupMessageFields(ui);

            ValidateYammerAction(groupMessageField.GroupID, "No selected group found in activity.");
            ValidateYammerAction(groupMessageField.Message, "No selected field found in activity.");

            var processPayload = await GetPayload(activityDO, containerId);

            var payloadMessageField = CrateManager.GetFieldByKey<StandardPayloadDataCM>(processPayload.CrateStorage, groupMessageField.Message);

            ValidateYammerAction(payloadMessageField, "No specified field found in activity.");

            await _yammer.PostMessageToGroup(authTokenDO.Token,
                groupMessageField.GroupID, payloadMessageField);

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

        private static GroupMessage GetGroupMessageFields(StandardConfigurationControlsCM ui)
        {
            var controls = new ActionUi();
            controls.ClonePropertiesFrom(ui);
            var groupMessage = new GroupMessage();
            groupMessage.GroupID = controls.Groups.Value;
            groupMessage.Message = controls.Message.Value;

            return groupMessage;
        }

        private async Task<Crate> CreateAvailableFieldsCrate(ActivityDO activityDO)
        {
            var curUpstreamFields =
                (await GetCratesByDirection<StandardDesignTimeFieldsCM>(activityDO, CrateDirection.Upstream))

                .Where(x => x.Label != "Available Groups")
                .SelectMany(x => x.Content.Fields)
                .ToArray();

            var availableFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate(
                    "Available Fields",
                    curUpstreamFields
                );

            return availableFieldsCrate;
        }

        private void ValidateYammerAction(string value, string exceptionMessage)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ApplicationException(exceptionMessage);
            }
        }
    }
}