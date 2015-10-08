using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;
using pluginSlack.Interfaces;
using pluginSlack.Services;

namespace pluginSlack.Actions
{
    public class Monitor_Channel_v1 : BasePluginAction
    {
        private readonly ISlackIntegration _slackIntegration;


        public Monitor_Channel_v1()
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

            return await ProcessConfigurationRequest(curActionDTO,
                x => ConfigurationEvaluator(x));
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
            var crateDesignTimeFields = CreateDesignChannelsCrate(channels);
            var crateEventSubscriptions = CreateEventSubscriptionCrate();
            curActionDTO.CrateStorage.CrateDTO.Add(crateControls);
            curActionDTO.CrateStorage.CrateDTO.Add(crateDesignTimeFields);
            curActionDTO.CrateStorage.CrateDTO.Add(crateEventSubscriptions);

            return await Task.FromResult<ActionDTO>(curActionDTO);
        }

        private CrateDTO CreateConfigurationCrate()
        {
            var fieldSelectDocusignTemplate = new DropdownListFieldDefinitionDTO()
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

            return PackControlsCrate(fieldSelectDocusignTemplate);
        }

        private CrateDTO CreateDesignChannelsCrate(IEnumerable<FieldDTO> channels)
        {
            var crate =
                _crate.CreateDesignTimeFieldsCrate(
                    "Available Channels",
                    channels.ToArray()
                );

            return crate;
        }

        private CrateDTO CreateEventSubscriptionCrate()
        {
            var subscriptions = new string[] {
                "Slack Outgoing Webhook"
            };

            return _crate.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                subscriptions.ToArray()
                );
        }
    }
}