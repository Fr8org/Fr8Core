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

namespace terminalYammer.Actions
{

    public class Post_To_Yammer_v1 : BaseTerminalAction
    {
        private readonly IYammerIntegration _yammerIntegration;

        public Post_To_Yammer_v1()
        {
            _yammerIntegration = new YammerIntegration();
        }

        public async Task<PayloadDTO> Run(ActionDO actionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            var processPayload = await GetProcessPayload(actionDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            var actionGroupId = ExtractControlFieldValue(actionDO, "Selected_Yammer_Group");
            if (string.IsNullOrEmpty(actionGroupId))
            {
                throw new ApplicationException("No selected group found in action.");
            }

            var actionFieldName = ExtractControlFieldValue(actionDO, "Select_Message_Field");
            if (string.IsNullOrEmpty(actionFieldName))
            {
                throw new ApplicationException("No selected field found in action.");
            }

            var payloadFields = ExtractPayloadFields(processPayload);

            var payloadMessageField = payloadFields.FirstOrDefault(x => x.Key == actionFieldName);
            if (payloadMessageField == null)
            {
                throw new ApplicationException("No specified field found in action.");
            }

            await _yammerIntegration.PostMessageToGroup(authTokenDO.Token,
                actionGroupId, payloadMessageField.Value);

            return processPayload;
        }

        private List<FieldDTO> ExtractPayloadFields(PayloadDTO processPayload)
        {
            var payloadDataCrates = Crate.FromDto(processPayload.CrateStorage).CratesOfType<StandardPayloadDataCM>();

            var result = new List<FieldDTO>();
            foreach (var payloadDataCrate in payloadDataCrates)
            {
                result.AddRange(payloadDataCrate.Content.AllValues());
            }

            return result;
        }

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
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
            var channels = await _yammerIntegration.GetGroupsList(oauthToken);

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

        private Crate PackCrate_ConfigurationControls()
        {
            var fieldSelectChannel = new DropDownList()
            {
                Label = "Select Yammer Group",
                Name = "Selected_Yammer_Group",
                Required = true,
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Groups",
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
                    "Available Groups",
                    channels.ToArray()
                );

            return crate;
        }

        private async Task<Crate> CreateAvailableFieldsCrate(ActionDO actionDO)
        {
            var curUpstreamFields =
                (await GetCratesByDirection<StandardDesignTimeFieldsCM>(actionDO, CrateDirection.Upstream))

                .Where(x => x.Label != "Available Groups")
                .SelectMany(x => x.Content.Fields)
                .ToArray();

            var availableFieldsCrate = Crate.CreateDesignTimeFieldsCrate(
                    "Available Fields",
                    curUpstreamFields
                );

            return availableFieldsCrate;
        }
    }
}