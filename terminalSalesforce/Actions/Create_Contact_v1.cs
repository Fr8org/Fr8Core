using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActionDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }
            
            var lastName = ExtractControlFieldValue(curActionDO, "lastName");
            if (string.IsNullOrEmpty(lastName))
            {
                return Error(payloadCrates, "No last name found in action.");
            }

            bool result = _salesforce.CreateContact(curActionDO, authTokenDO);

            return Success(payloadCrates);
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
                Name = "firstName"

            };
            var lastName = new TextBox()
            {
                Label = "Last Name",
                Name = "lastName",
                Required = true
            };

            var mobileNumber = new TextBox()
            {
                Label = "Mobile Phone",
                Name = "mobilePhone",
                Required = true
            };

            var email = new TextBox()
            {
                Label = "Email",
                Name = "email",
                Required = true
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