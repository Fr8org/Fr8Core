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
        private readonly ICrate _crateService;

        public MapFields_v1()
        {
            _crateService = ObjectFactory.GetInstance<ICrate>();
        }

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
        private CrateDTO CreateStandartConfigurationControls()
        {
            var fieldFilterPane = new FieldDefinitionDTO()
            {
                FieldLabel = "Configure Mapping",
                Type = "mappingPane",
                Name = "Selected_Mapping",
                Required = true
            };

            var fields = new List<FieldDefinitionDTO>()
            {
                fieldFilterPane
            };

            var crateControls = _crateService.Create(
                "Configuration_Controls",
                JsonConvert.SerializeObject(fields),
                "Standard Configuration Controls"
                );

            return crateControls;
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

            var curUpstreamFields = new List<MappingFieldConfigurationDTO>();
            FillCrateConfigureList(curUpstreamActivities.OfType<ActionDO>(), curUpstreamFields);

            // TODO: test purposes only! to be removed when entire PB gets integrated.
            if (curUpstreamFields.Count == 0)
            {
                curUpstreamFields.Add(new MappingFieldConfigurationDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "[Test].[UpStreamField_01]"
                });

                curUpstreamFields.Add(new MappingFieldConfigurationDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "[Test].[UpStreamField_02]"
                });
            }

            var curDownstreamFields = new List<MappingFieldConfigurationDTO>();
            FillCrateConfigureList(curUpstreamActivities.OfType<ActionDO>(), curDownstreamFields);

            // TODO: test purposes only! to be removed when entire PB gets integrated.
            if (curDownstreamFields.Count == 0)
            {
                curDownstreamFields.Add(new MappingFieldConfigurationDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "[Test].[DownStreamField_01]"
                });

                curDownstreamFields.Add(new MappingFieldConfigurationDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "[Test].[DownStreamField_02]"
                });
            }

            if (curUpstreamFields.Count == 0 || curDownstreamFields.Count == 0)
            {
                throw new ApplicationException("This action couldn't find either source fields or target fields (or both). "
                    + "Try configuring some Actions first, then try this page again.");
            }

            var curUpstreamJson = JsonConvert.SerializeObject(curUpstreamFields, JsonSettings.CamelCase);
            var curDownstreamJson = JsonConvert.SerializeObject(curDownstreamFields, JsonSettings.CamelCase);

            var curConfigurationControlsCrage = CreateStandartConfigurationControls();

            var curResultDTO = new CrateStorageDTO()
            {
                CrateDTO = new List<CrateDTO>()
                {
                    curConfigurationControlsCrage,

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