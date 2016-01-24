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
        ISalesforceManager _salesforce = new SalesforceManager();

        public override async Task<ActionDO> Configure(ActionDO curActionDO,AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            return ConfigurationRequestType.Initial;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO = null)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();

                AddTextSourceControl(updater.CrateStorage, "First Name", "firstName",
                    "Upstream Terminal-Provided Fields", addRequestConfigEvent: false);
                AddTextSourceControl(updater.CrateStorage, "Last Name", "lastName",
                    "Upstream Terminal-Provided Fields", addRequestConfigEvent: false, required:true);
                AddTextSourceControl(updater.CrateStorage, "Mobile Phone", "mobilePhone",
                    "Upstream Terminal-Provided Fields", addRequestConfigEvent: false, required:true);
                AddTextSourceControl(updater.CrateStorage, "Email", "email",
                    "Upstream Terminal-Provided Fields", addRequestConfigEvent: false, required:true);

                updater.CrateStorage.Add(await CreateAvailableFieldsCrate(curActionDO));
            }

            return await Task.FromResult(curActionDO);
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActionDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            var firstName = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "firstName");
            var lastName = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "lastName");
            var mobilePhone = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "mobilePhone");
            var email = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "email");
            if (string.IsNullOrEmpty(lastName))
            {
                return Error(payloadCrates, "No last name found in action.");
            }

            var contact = new ContactDTO
            {
                FirstName = firstName,
                LastName = lastName,
                MobilePhone = mobilePhone,
                Email = email
            };

            bool result = await _salesforce.CreateObject(contact, "Contact", _salesforce.CreateForceClient(authTokenDO));

            if (result)
            {
                return Success(payloadCrates);
            }

            return Error(payloadCrates, "Contact creation is failed");
        }
    }
}