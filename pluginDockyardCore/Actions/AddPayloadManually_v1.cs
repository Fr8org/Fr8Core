﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;

namespace pluginDockyardCore.Actions
{
    public class AddPayloadManually_v1 : BasePluginAction
    {
        public async Task<ActionDTO> Configure(ActionDTO curActionDataPackageDTO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDTO, ConfigurationEvaluator);
        }

        protected override Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            var crateStrorageDTO = AssembleCrateStorage(configurationControlsCrate);
            curActionDTO.CrateStorage = crateStrorageDTO;

            return Task.FromResult(curActionDTO);
        }

        protected override Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            var controlsCrate = curActionDTO.CrateStorage.CrateDTO
                .SingleOrDefault(x => x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME);
            if (controlsCrate == null)
            {
                throw new ApplicationException("Could not find ControlsConfiguration crate.");
            }

            var controlsMS = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(controlsCrate.Contents);

            var fieldListControl = controlsMS.Controls
                .SingleOrDefault(x => x.Type == ControlTypes.FieldList);

            if (fieldListControl == null)
            {
                throw new ApplicationException("Could not find FieldListControl.");
            }

            _crate.RemoveCrateByLabel(
                curActionDTO.CrateStorage.CrateDTO,
                "ManuallyAddedPayload"
                );


            var userDefinedPayload = JsonConvert.DeserializeObject<List<FieldDTO>>(fieldListControl.Value);

            curActionDTO.CrateStorage.CrateDTO.Add(
                _crate.CreateDesignTimeFieldsCrate(
                    "ManuallyAddedPayload",
                    userDefinedPayload.ToArray()
                    )
                );

            return Task.FromResult(curActionDTO);
        }

        private CrateDTO CreateControlsCrate()
        {
            var fieldFilterPane = new ControlDefinitionDTO(ControlTypes.FieldList)
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

        private ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            if (curActionDTO.CrateStorage == null
                || curActionDTO.CrateStorage.CrateDTO == null
                || curActionDTO.CrateStorage.CrateDTO.Count == 0)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}