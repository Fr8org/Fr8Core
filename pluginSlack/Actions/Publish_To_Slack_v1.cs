using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using PluginBase;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;
using pluginSlack.Interfaces;
using pluginSlack.Services;

namespace pluginSlack.Actions
{

    public class Publish_To_Slack_v1 : BasePluginAction
    {
        private readonly ISlackIntegration _slackIntegration;


        public Publish_To_Slack_v1()
        {
            _slackIntegration = new SlackIntegration();
        }

        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            if (IsEmptyAuthToken(curActionDTO))
            {
                AppendDockyardAuthenticationCrate(
                    curActionDTO,
                    AuthenticationMode.ExternalMode);

                return curActionDTO;
            }

            RemoveAuthenticationCrate(curActionDTO);

            return await ProcessConfigurationRequest(curActionDTO, x => ConfigurationEvaluator(x));
        }

        private ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            var crateStorage = curActionDTO.CrateStorage;

            if (crateStorage.CrateDTO.Count == 0)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDTO> InitialConfigurationResponse(
            ActionDTO curActionDTO)
        {
            var oauthToken = curActionDTO.AuthToken.Token;
            var channels = await _slackIntegration.GetChannelList(oauthToken);

            var crateControls = CreateConfigurationCrate();
            var crateAvailableChannels = CreateAvailableChannelsCrate(channels);
            var crateAvailableFields = await CreateAvailableFieldsCrate(curActionDTO);
            curActionDTO.CrateStorage.CrateDTO.Add(crateControls);
            curActionDTO.CrateStorage.CrateDTO.Add(crateAvailableChannels);
            curActionDTO.CrateStorage.CrateDTO.Add(crateAvailableFields);

            return curActionDTO;
        }

        private CrateDTO CreateConfigurationCrate()
        {
            var fieldSelectChannel = new DropdownListFieldDefinitionDTO()
            {
                Label = "Select Slack Channel",
                Name = "Selected_Slack_Channel",
                Required = true,
                Events = new List<FieldEvent>()
                {
                    new FieldEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Channels",
                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                }
            };

            var fieldSelectMessageField = new DropdownListFieldDefinitionDTO()
            {
                Label = "Select Message Field",
                Name = "Select_Message_Field",
                Required = true,
                Events = new List<FieldEvent>()
                {
                    new FieldEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Fields",
                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                }
            };

            return PackControlsCrate(fieldSelectChannel, fieldSelectMessageField);
        }

        private CrateDTO CreateAvailableChannelsCrate(IEnumerable<FieldDTO> channels)
        {
            var crate =
                _crate.CreateDesignTimeFieldsCrate(
                    "Available Channels",
                    channels.ToArray()
                );

            return crate;
        }

        private async Task<CrateDTO> CreateAvailableFieldsCrate(ActionDTO actionDTO)
        {
            var curUpstreamFields =
                (await GetDesignTimeFields(actionDTO.Id, GetCrateDirection.Upstream))
                .Fields
                .ToArray();

            var availableFieldsCrate =
                _crate.CreateDesignTimeFieldsCrate(
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