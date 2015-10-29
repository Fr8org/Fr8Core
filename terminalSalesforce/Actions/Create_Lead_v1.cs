using Data.Interfaces.DataTransferObjects;
using StructureMap;
using terminalSalesforce.Infrastructure;
using System.Threading.Tasks;
using Data.Interfaces.Manifests;
using Salesforce.Common;
using terminalSalesforce.Services;
using TerminalBase.Infrastructure;
using System.Collections.Generic;
using Data.Entities;
using TerminalBase.BaseClasses;
using System;

namespace terminalSalesforce.Actions
{
    public class Create_Lead_v1 : BasePluginAction
    {
        ISalesforceIntegration _salesforce = new SalesforceIntegration();

        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            if (NeedsAuthentication(curActionDTO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActionDTO, x => ConfigurationEvaluator(x));
        }

        public object Activate(ActionDO curActionDO)
        {
            //not implemented currently
            return null;
        }

        public object Deactivate(ActionDO curActionDO)
        {
            //not implemented currentlys
            return "Deactivated";
        }

        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            PayloadDTO processPayload = null;
          
                processPayload = await GetProcessPayload(curActionDTO.ProcessId);

                if (NeedsAuthentication(curActionDTO))
                {
                    throw new ApplicationException("No AuthToken provided.");
                }


                var lastName = ExtractControlFieldValue(curActionDTO,"lastName");
                if (string.IsNullOrEmpty(lastName))
                {
                    throw new ApplicationException("No last name found in action.");
                }

                var company = ExtractControlFieldValue(curActionDTO, "companyName");
                if (string.IsNullOrEmpty(company))
                {
                    throw new ApplicationException("No company name found in action.");
                }

                bool result = _salesforce.CreateLead(curActionDTO);
           
          
            return processPayload;
        }

        private ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            return ConfigurationRequestType.Initial;
        }

        protected override async Task<ActionDTO> InitialConfigurationResponse(
         ActionDTO curActionDTO)
        {
            var firstNameCrate = new TextBoxControlDefinitionDTO()
            {
                Label = "First Name",
                Name = "firstName",
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }

            };
            var lastNAme = new TextBoxControlDefinitionDTO()
            {
                Label = "Last Name",
                Name = "lastName",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };
            var company = new TextBoxControlDefinitionDTO()
            {
                Label = "Company ",
                Name = "companyName",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };

            var controls = PackControlsCrate(firstNameCrate, lastNAme, company);
            curActionDTO.CrateStorage.CrateDTO.Add(controls);

            return await Task.FromResult<ActionDTO>(curActionDTO);
        }
    }
}