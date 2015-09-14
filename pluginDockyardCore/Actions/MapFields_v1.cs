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
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;
using Utilities;

namespace pluginDockyardCore.Actions
{
    public class MapFields_v1 : BasePluginAction
    {
        private class CrateConfigurationDTO
        {
            public string Id { get; set; }
            public string Label { get; set; }
        }

        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public ActionProcessResultDTO Execute(ActionDO actionDO)
        {
            var curFieldMappingSettings = actionDO.CrateStorageDTO()
                .CratesDTO
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
                    ManifestType = "Payload Data"
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
            List<CrateConfigurationDTO> crateConfigList)
        {
            foreach (var curAction in actions)
            {
                var curCrateStorage = curAction.CrateStorageDTO();
                foreach (var curCrate in curCrateStorage.CratesDTO)
                {
                    crateConfigList.Add(new CrateConfigurationDTO()
                    {
                        Id = curCrate.Id,
                        Label = curCrate.Label
                    });
                }
            }
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override CrateStorageDTO InitialConfigurationResponse(ActionDTO actionDTO)
        {
            var curActionDO = Mapper.Map<ActionDO>(actionDTO);
            var curActivityService = ObjectFactory.GetInstance<IActivity>();

            var curUpstreamActivities = curActivityService.GetUpstreamActivities(curActionDO);
            var curDownstreamActivities = curActivityService.GetDownstreamActivities(curActionDO);

            var curUpstreamFields = new List<CrateConfigurationDTO>();
            FillCrateConfigureList(curUpstreamActivities.OfType<ActionDO>(), curUpstreamFields);

            var curDownstreamFields = new List<CrateConfigurationDTO>();
            FillCrateConfigureList(curUpstreamActivities.OfType<ActionDO>(), curDownstreamFields);

            if (curUpstreamFields.Count == 0 || curDownstreamFields.Count == 0)
            {
                throw new ApplicationException("This action couldn't find either source fields or target fields (or both). "
                    + "Try configuring some Actions first, then try this page again.");
            }

            var curUpstreamJson = JsonConvert.SerializeObject(curUpstreamFields, JsonSettings.CamelCase);
            var curDownstreamJson = JsonConvert.SerializeObject(curDownstreamFields, JsonSettings.CamelCase);

            var curResultDTO = new CrateStorageDTO()
            {
                CratesDTO = new List<CrateDTO>()
                {
                    new CrateDTO()
                    {
                        Id = "Upstream Plugin-Provided Fields",
                        Label = "Upstream Plugin-Provided Fields",
                        Contents = curUpstreamJson
                    },

                    new CrateDTO()
                    {
                        Id = "Downstream Plugin-Provided Fields",
                        Label = "Downstream Plugin-Provided Fields",
                        Contents = curDownstreamJson
                    }
                }
            };

            curActionDO.UpdateCrateStorageDTO(curResultDTO.CratesDTO);

            return curResultDTO;
        }

        /// <summary>
        /// ConfigurationEvaluator always returns Initial,
        /// since Initial and FollowUp phases are the same for current action.
        /// </summary>
        private ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDO)
        {
            return ConfigurationRequestType.Initial;
        }
    }
}