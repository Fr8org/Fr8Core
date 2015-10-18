using Data.Interfaces.DataTransferObjects;
using StructureMap;
using pluginSalesforce.Infrastructure;
using PluginUtilities.BaseClasses;
using System.Threading.Tasks;
using Data.Interfaces.ManifestSchemas;
using Salesforce.Common;
using pluginSalesforce.Services;
using PluginBase.Infrastructure;
using System.Collections.Generic;
using Data.Entities;
namespace pluginSalesforce.Actions
{
    public class Create_Lead_v1 : BasePluginAction
    {
        ISalesforceIntegration _salesforce = new SalesforceIntegration();       

        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            if (IsEmptyAuthToken(curActionDTO))
            {
                AppendDockyardAuthenticationCrate(
                    curActionDTO,
                    AuthenticationMode.ExternalMode);

                return curActionDTO;
            }

           // RemoveAuthenticationCrate(curActionDTO);

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
            var firstNameCrate = new TextBoxControlDefinitionDTO()
            {
                Label = "First Name",
                Name = "firstName",

            };
            var lastNAme = new TextBoxControlDefinitionDTO()
            {
                Label = "Last Name",
                Name = "lastName",
                Required = true
            };
            var company = new TextBoxControlDefinitionDTO()
            {
                Label = "Company ",
                Name = "CompanyName",
                Required = true
            };
            var buttonControl = new ButtonControlDefinisionDTO()
            {
                Label = "Button Control",
                Name = "button_new",
                CssClass = "btn red",
                Events = new List<ControlEvent>() { new ControlEvent("onClick", "requestConfig") }
            };

            var controls = PackControlsCrate(firstNameCrate, lastNAme, company, buttonControl);
            curActionDTO.CrateStorage.CrateDTO.Add(controls);

            return await Task.FromResult<ActionDTO>(curActionDTO);
        }

        protected override async Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            bool result = _salesforce.CreateLead(curActionDTO);
            var curActionDO = AutoMapper.Mapper.Map<ActionDO>(curActionDTO);
            var controls = PackCrate_ErrorTextBox("", "Lead Created Successfully");
            Action.AddCrate(curActionDO, controls);
            var curCrateStorageDTO = curActionDO.CrateStorageDTO();
            curActionDTO.CrateStorage = curCrateStorageDTO;        
            return await Task.FromResult<ActionDTO>(curActionDTO);
        }       
    }
}