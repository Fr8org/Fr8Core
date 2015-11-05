using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using TerminalBase.Infrastructure;
using TerminalBase.BaseClasses;
using Data.Entities;

namespace terminalFr8Core.Actions
{
    public class AddPayloadManually_v1 : BasePluginAction
    {
        public async Task<PayloadDTO> Run(ActionDO curActionDO, int containerId, AuthorizationTokenDO authTokenDO = null)
        {
            var processPayload = await GetProcessPayload(containerId);

            var controlsCrate = curActionDO.CrateStorageDTO().CrateDTO
                .SingleOrDefault(x => x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_MANIFEST_NAME);
            if (controlsCrate == null)
            {
                throw new ApplicationException("Could not find ControlsConfiguration crate.");
            }

            var controlsMS = JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(controlsCrate.Contents);

            var fieldListControl = controlsMS.Controls
                .SingleOrDefault(x => x.Type == ControlTypes.FieldList);

            if (fieldListControl == null)
            {
                throw new ApplicationException("Could not find FieldListControl.");
            }

            var userDefinedPayload = JsonConvert.DeserializeObject<List<FieldDTO>>(fieldListControl.Value);

            var cratePayload = Crate.Create(
                "Manual Payload Data",
                JsonConvert.SerializeObject(userDefinedPayload),
                CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME,
                CrateManifests.STANDARD_PAYLOAD_MANIFEST_ID
                );

            processPayload.UpdateCrateStorageDTO(new List<CrateDTO>() { cratePayload });

            return processPayload;
        }

        public async Task<ActionDO> Configure(ActionDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO=null)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            var crateStrorageDTO = AssembleCrateStorage(configurationControlsCrate);
            curActionDO.UpdateCrateStorageDTO(crateStrorageDTO.CrateDTO);

            return Task.FromResult(curActionDO);
        }

        protected override Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO = null)
        {
            var controlsCrate = curActionDO.CrateStorageDTO().CrateDTO
                .SingleOrDefault(x => x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_MANIFEST_NAME);
            if (controlsCrate == null)
            {
                throw new ApplicationException("Could not find ControlsConfiguration crate.");
            }

            var controlsMS = JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(controlsCrate.Contents);

            var fieldListControl = controlsMS.Controls
                .SingleOrDefault(x => x.Type == ControlTypes.FieldList);

            if (fieldListControl == null)
            {
                throw new ApplicationException("Could not find FieldListControl.");
            }

            Crate.RemoveCrateByLabel(
                curActionDO.CrateStorageDTO().CrateDTO,
                "ManuallyAddedPayload"
                );


            var userDefinedPayload = JsonConvert.DeserializeObject<List<FieldDTO>>(fieldListControl.Value);
            userDefinedPayload.ForEach(x => x.Value = x.Key);

            curActionDO.CrateStorageDTO().CrateDTO.Add(
                Crate.CreateDesignTimeFieldsCrate(
                    "ManuallyAddedPayload",
                    userDefinedPayload.ToArray()
                    )
                );

            return Task.FromResult(curActionDO);
        }

        private CrateDTO CreateControlsCrate()
        {
            var fieldFilterPane = new FieldListControlDefinitionDTO
            {
                Label = "Fill the values for other actions",
                Name = "Selected_Fields",
                Required = true,
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                }
            };

            return PackControlsCrate(fieldFilterPane);
        }

        private ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (curActionDO.CrateStorage == null
                || curActionDO.CrateStorageDTO().CrateDTO == null
                || curActionDO.CrateStorageDTO().CrateDTO.Count == 0)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}