using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using StructureMap;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;
using Utilities;

namespace pluginDockyardCore.Actions
{
    public class MapFields_v1 : BasePluginAction
    {

        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public ActionProcessResultDTO Execute(ActionDO actionDO)
        {
            var curFieldMappingSettings = actionDO.CrateStorageDTO()
                .CrateDTO
                .Where(x => x.Label == "Field Mappings")
                .FirstOrDefault();

            if (curFieldMappingSettings == null)
            {
                throw new ApplicationException("No Field Mapping cratefound for current action.");
            }

            var curFieldMappingJson = JsonConvert.SerializeObject(curFieldMappingSettings, JsonSettings.CamelCase);

            var crates = new List<CrateDTO>()
            {
                new CrateDTO()
                {
                    Contents = curFieldMappingJson,
                    Label = "Payload",
                    ManifestType = "Standard Payload Data"
                }
            };

            ((ActionListDO)actionDO.ParentActivity).Process.UpdateCrateStorageDTO(crates);

            return new ActionProcessResultDTO() { Success = true };
        }

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public CrateStorageDTO Configure(ActionDTO actionDTO)
        {
            return ProcessConfigurationRequest(actionDTO, ConfigurationEvaluator);
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
            var fieldFilterPane = new FieldDefinitionDTO()
            {
                FieldLabel = "Configure Mapping",
                Type = "mappingPane",
                Name = "Selected_Mapping",
                Required = true
            };

            return PackControlsCrate(fieldFilterPane);
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override CrateStorageDTO InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            ActionDO curActionDO = _action.MapFromDTO(curActionDTO);

            var curUpstreamFields = GetDesignTimeFields(curActionDO, GetCrateDirection.Upstream).Fields.ToArray();

            var curDownstreamFields = GetDesignTimeFields(curActionDO, GetCrateDirection.Downstream).Fields.ToArray();

            if (curUpstreamFields.Length == 0 || curDownstreamFields.Length == 0)
            {
                //temporarily disabling this exception because it's disrupting debugging. it will get fixed properly in 1085
                //throw new ApplicationException("This action couldn't find either source fields or target fields (or both). "
                   // + "Try configuring some Actions first, then try this page again.");
            }

            //Pack the merged fields into 2 new crates that can be used to populate the dropdowns in the MapFields UI
            CrateDTO downstreamFieldsCrate = _crate.CreateDesignTimeFieldsCrate("Downstream Plugin-Provided Fields", curDownstreamFields);
            CrateDTO upstreamFieldsCrate = _crate.CreateDesignTimeFieldsCrate("Upstream Plugin-Provided Fields", curUpstreamFields);

            var curConfigurationControlsCrate = CreateStandardConfigurationControls();

            return AssembleCrateStorage(downstreamFieldsCrate, upstreamFieldsCrate, curConfigurationControlsCrate);
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
                .DeserializeObject<StandardDesignTimeFieldsMS>(upStreamFieldsCrate.Contents);

            var downStreamFields = JsonConvert
                .DeserializeObject<StandardDesignTimeFieldsMS>(downStreamFieldsCrate.Contents);

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