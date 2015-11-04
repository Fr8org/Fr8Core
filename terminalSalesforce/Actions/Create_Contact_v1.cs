using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;

namespace terminalSalesforce.Actions
{
    public class Create_Contact_v1 : BasePluginAction
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

            processPayload = await GetProcessPayload(curActionDTO.ContainerIdId);

            if (NeedsAuthentication(curActionDTO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }


            var lastName = ExtractControlFieldValue(curActionDTO, "lastName");
            if (string.IsNullOrEmpty(lastName))
            {
                throw new ApplicationException("No last name found in action.");
            }


            bool result = _salesforce.CreateContact(curActionDTO);


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
            var lastName = new TextBoxControlDefinitionDTO()
            {
                Label = "Last Name",
                Name = "lastName",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };

            var mobileNumber = new TextBoxControlDefinitionDTO()
            {
                Label = "Mobile Phone",
                Name = "mobilePhone",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };

            var email = new TextBoxControlDefinitionDTO()
            {
                Label = "Email",
                Name = "email",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };

            var controls = PackControlsCrate(firstNameCrate, lastName, mobileNumber, email);
            curActionDTO.CrateStorage.CrateDTO.Add(controls);

            return await Task.FromResult<ActionDTO>(curActionDTO);
        }
    }
}