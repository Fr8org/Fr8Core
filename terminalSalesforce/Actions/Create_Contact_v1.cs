using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Control;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;

namespace terminalSalesforce.Actions
{
    public class Create_Contact_v1 : BaseTerminalAction
    {
        ISalesforceIntegration _salesforce = new SalesforceIntegration();

        public override async Task<ActionDO> Configure(ActionDO curActionDO,AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActionDO, x => ConfigurationEvaluator(x), authTokenDO);
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            PayloadDTO processPayload = null;

            processPayload = await GetProcessPayload(curActionDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }


            var lastName = ExtractControlFieldValue(curActionDO, "lastName");
            if (string.IsNullOrEmpty(lastName))
            {
                throw new ApplicationException("No last name found in action.");
            }


            bool result = _salesforce.CreateContact(curActionDO, authTokenDO);


            return processPayload;
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            return ConfigurationRequestType.Initial;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO= null)
        {
            var firstNameCrate = new TextBox()
            {
                Label = "First Name",
                Name = "firstName",
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }

            };
            var lastName = new TextBox()
            {
                Label = "Last Name",
                Name = "lastName",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };

            var mobileNumber = new TextBox()
            {
                Label = "Mobile Phone",
                Name = "mobilePhone",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };

            var email = new TextBox()
            {
                Label = "Email",
                Name = "email",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };

            var controls = PackControlsCrate(firstNameCrate, lastName, mobileNumber, email);
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(controls);
            }

            return await Task.FromResult(curActionDO);
        }
    }
}