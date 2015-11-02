using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities.Configuration.Azure;

namespace terminalFr8Core.Actions
{
    // The generic interface inheritance.
    public class Select_Fr8_Object_v1 : BasePluginAction
    {

        // configure the action will return the initial UI crate 
        public async Task<ActionDTO> Configure(ActionDTO curActionDataPackageDTO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDTO, ConfigurationEvaluator);
        }

        protected override Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            // build a controls crate to render the dropdown build
            var configurationControlsCrate = CreateControlsCrate();

            var crateDesignTimeFields = PackFr8ObjectCrate();
            var crateStrorageDTO = AssembleCrateStorage(configurationControlsCrate, crateDesignTimeFields);
            curActionDTO.CrateStorage = crateStrorageDTO;

            return Task.FromResult(curActionDTO);
        }

        private string GetSelectedFr8Object(ActionDTO curActionDTO)
        {
            var controlsCrates = Crate.GetCratesByManifestType(CrateManifests.STANDARD_CONF_CONTROLS_MANIFEST_NAME,
                curActionDTO.CrateStorage);
            var curFr8Selected_Object = Crate.GetElementByKey(controlsCrates, key: "Selected_Fr8_Object",
                keyFieldName: "name")
                .Select(e => (string)e["value"])
                .FirstOrDefault(s => !string.IsNullOrEmpty(s));
            return curFr8Selected_Object;
        }

        protected override async Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {

            string curSelectedFr8Object = GetSelectedFr8Object(curActionDTO);

            if (!string.IsNullOrEmpty(curSelectedFr8Object))
            {
                var curActionDO = Mapper.Map<ActionDO>(curActionDTO);
                var fr8ObjectCrateDTO = await GetDesignTimeFieldsCrateOfSelectedFr8Object(curSelectedFr8Object);

                // Change the label of the design time field crate in the current context.
                var designTimeControlName = "Select Fr8 Object Properties";
                fr8ObjectCrateDTO.Label = designTimeControlName;
                Crate.AddOrReplaceCrate(designTimeControlName, curActionDO, fr8ObjectCrateDTO);
                curActionDTO = Mapper.Map<ActionDTO>(curActionDO);
            }
            return await Task.FromResult(curActionDTO);
        }

        private CrateDTO CreateControlsCrate()
        {

            var fieldSelectFr8Object = new DropDownListControlDefinitionDTO()
            {
                Label = "Select Fr8 Object",
                Name = "Selected_Fr8_Object",
                Value = "",
                Required = true,
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Select Fr8 Object",
                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME,
                }
            };

            return PackControlsCrate(fieldSelectFr8Object);
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

        private CrateDTO PackFr8ObjectCrate()
        {
            var fields = new List<FieldDTO> {
                    new FieldDTO(){
                       Key = "Routes",
                       Value = "19"
                   },
                   new FieldDTO(){
                       Key = "Containers",
                       Value = "21"
                   }
            }.ToArray();

            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate(
                "Select Fr8 Object",
                fields);
            return createDesignTimeFields;
        }

        // Get the Design time fields crate.
        private async Task<CrateDTO> GetDesignTimeFieldsCrateOfSelectedFr8Object(string fr8Object)
        {
            var fr8ObjectPropertiesList = new List<FieldDTO>();

            var httpClient = new HttpClient();

            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "manifests/"
                + Int32.Parse(fr8Object);
            using (var response = await httpClient.GetAsync(url))
            {
                var content = await response.Content.ReadAsAsync<CrateDTO>();
                return content;
            }
        }

    }
}