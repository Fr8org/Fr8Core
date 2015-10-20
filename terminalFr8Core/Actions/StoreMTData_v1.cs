using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Newtonsoft.Json;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;

namespace pluginDockyardCore.Actions
{
    public class StoreMTData_v1 : BasePluginAction
    {
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await ProcessConfigurationRequest(curActionDTO, actionDTO => ConfigurationRequestType.Initial);
        }

        public Task<object> Activate(ActionDTO curActionDTO)
        {
            //No activation logic decided yet
            return null;
        }

        public Task<object> Deactivate(ActionDTO curDataPackage)
        {
            //No deactivation logic decided yet
            return null;
        }

        public async Task<PayloadDTO> Run(ActionDTO actionDto)
        {
            //Waiting for sergey changes to be merged in
            return null;
        }

        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDTO);

            CrateDTO curMergedUpstreamRunTimeObjects =
                await MergeUpstreamFields(curActionDO.Id, "Available Run-Time Objects");

            var fieldSelectObjectTypes = new DropDownListControlDefinitionDTO()
            {
                Label = "Save Which Data Types?",
                Name = "Save Object Name",
                Required = true,
                Events = new List<ControlEvent>(),
                Source = new FieldSourceDTO
                {
                    Label = curMergedUpstreamRunTimeObjects.Label,
                    ManifestType = curMergedUpstreamRunTimeObjects.ManifestType
                }
            };

            var curConfigurationControlsCrate = PackControlsCrate(fieldSelectObjectTypes);

            FieldDTO[] curSelectedFields =
                JsonConvert.DeserializeObject<StandardDesignTimeFieldsCM>(curMergedUpstreamRunTimeObjects.Contents)
                    .Fields.Select(field => new FieldDTO {Key = field.Key, Value = field.Value})
                    .ToArray();

            var curSelectedObjectType = Crate.CreateDesignTimeFieldsCrate("SelectedObjectTypes", curSelectedFields);

            curActionDO.UpdateCrateStorageDTO(new List<CrateDTO>
            {
                curMergedUpstreamRunTimeObjects,
                curConfigurationControlsCrate,
                curSelectedObjectType
            });

            return Mapper.Map<ActionDTO>(curActionDO);
        }
    }
}