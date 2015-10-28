using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Enums;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using TerminalBase.Infrastructure;
using TerminalBase.BaseClasses;

namespace terminalFr8Core.Actions
{
    public class MapFields_v1 : BasePluginAction
    {
        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public async Task<PayloadDTO> Execute(ActionDTO actionDto)
        {
            var curControlsCrate = actionDto
                .CrateStorage
                .CrateDTO
                .FirstOrDefault(x => x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME);

            if (curControlsCrate == null)
            {
                throw new ApplicationException("No controls crate found.");
            }

            var curControlsMS = JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(curControlsCrate.Contents);
            var curMappingControl = curControlsMS.Controls
                .FirstOrDefault(x => x.Name == "Selected_Mapping");

            if (curMappingControl == null || string.IsNullOrEmpty(curMappingControl.Value))
            {
                throw new ApplicationException("No Selected_Mapping control found.");
            }

            var mappedFields = JsonConvert.DeserializeObject<List<FieldDTO>>(curMappingControl.Value);
            mappedFields = mappedFields.Where(x => x.Key != null && x.Value != null).ToList();

            var processPayload = await GetProcessPayload(actionDto.ProcessId);

            var actionPayloadCrates = new List<CrateDTO>()
            {
                Crate.Create("MappedFields",
                    JsonConvert.SerializeObject(mappedFields),
                    CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME,
                    CrateManifests.STANDARD_PAYLOAD_MANIFEST_ID)
            };

            processPayload.UpdateCrateStorageDTO(actionPayloadCrates);

            return processPayload;
        }

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public async Task<ActionDTO> Configure(ActionDTO actionDTO)
        {
            return await ProcessConfigurationRequest(actionDTO, ConfigurationEvaluator);
        }

        private void FillCrateConfigureList(IEnumerable<ActionDO> actions,
            List<MappingFieldConfigurationDTO> crateConfigList)
        {
            foreach (var curAction in actions)
            {
                var curCrateStorage = curAction.CrateStorageDTO();
                foreach (var curCrate in curCrateStorage.CrateDTO)
                {
                    crateConfigList.Add(new MappingFieldConfigurationDTO()
                    {
                        Id = curCrate.Id,
                        Label = curCrate.Label
                    });
                }
            }
        }

        /// <summary>
        /// Create configuration controls crate.
        /// </summary>
        private CrateDTO CreateStandardConfigurationControls()
        {
            var fieldFilterPane = new MappingPaneControlDefinitionDTO()
            {
                Label = "Configure Mapping",
                Name = "Selected_Mapping",
                Required = true
            };

            return PackControlsCrate(fieldFilterPane);
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            CrateDTO getErrorMessageCrate = null; 

            var curUpstreamFields =
                (await GetDesignTimeFields(curActionDTO.Id, GetCrateDirection.Upstream))
                .Fields
                .ToArray();

            var curDownstreamFields =
                (await GetDesignTimeFields(curActionDTO.Id, GetCrateDirection.Downstream))
                .Fields
                .ToArray();

            if (curUpstreamFields.Length == 0 || curDownstreamFields.Length == 0)
            {
                getErrorMessageCrate = GetTextBoxControlForDisplayingError("MapFieldsErrorMessage",
                         "This action couldn't find either source fields or target fields (or both). " +
                        "Try configuring some Actions first, then try this page again.");
                curActionDTO.CurrentView = "MapFieldsErrorMessage";
            }

            //Pack the merged fields into 2 new crates that can be used to populate the dropdowns in the MapFields UI
            CrateDTO downstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Downstream Plugin-Provided Fields", curDownstreamFields);
            CrateDTO upstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Upstream Plugin-Provided Fields", curUpstreamFields);

            var curConfigurationControlsCrate = CreateStandardConfigurationControls();

            var cratesToAssemble = new List<CrateDTO>()
            {
                downstreamFieldsCrate,
                upstreamFieldsCrate,
                curConfigurationControlsCrate
            };

            if (getErrorMessageCrate != null)
            {
                cratesToAssemble.Add(getErrorMessageCrate);
            }

            curActionDTO.CrateStorage = AssembleCrateStorage(cratesToAssemble.ToArray());
            return curActionDTO;
        }

        /// <summary>
        /// Check if initial configuration was requested.
        /// </summary>
        private bool CheckIsInitialConfiguration(ActionDTO curAction)
        {
            // Check nullability for CrateStorage and Crates array length.
            if (curAction.CrateStorage == null
                || curAction.CrateStorage.CrateDTO == null
                || curAction.CrateStorage.CrateDTO.Count == 0)
            {
                return true;
            }

            // Check nullability of Upstream and Downstream crates.
            var upStreamFieldsCrate = curAction.CrateStorage.CrateDTO.FirstOrDefault(
                x => x.Label == "Upstream Plugin-Provided Fields"
                    && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME);

            var downStreamFieldsCrate = curAction.CrateStorage.CrateDTO.FirstOrDefault(
                x => x.Label == "Downstream Plugin-Provided Fields"
                    && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME);

            if (upStreamFieldsCrate == null
                || string.IsNullOrEmpty(upStreamFieldsCrate.Contents)
                || downStreamFieldsCrate == null
                || string.IsNullOrEmpty(downStreamFieldsCrate.Contents))
            {
                return true;
            }

            // Check if Upstream and Downstream ManifestSchemas contain empty set of fields.
            var upStreamFields = JsonConvert
                .DeserializeObject<StandardDesignTimeFieldsCM>(upStreamFieldsCrate.Contents);

            var downStreamFields = JsonConvert
                .DeserializeObject<StandardDesignTimeFieldsCM>(downStreamFieldsCrate.Contents);

            if (upStreamFields.Fields == null
                || upStreamFields.Fields.Count == 0
                || downStreamFields.Fields == null
                || downStreamFields.Fields.Count == 0)
            {
                return true;
        }

            // If all rules are passed, then it is not an initial configuration request.
            return false;
        }
        /// <summary>
        /// ConfigurationEvaluator always returns Initial,
        /// since Initial and FollowUp phases are the same for current action.
        /// </summary>
        private ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDO)
        {
            if (CheckIsInitialConfiguration(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }
            else
            {
                return ConfigurationRequestType.Followup;
            }
        }
    }
}