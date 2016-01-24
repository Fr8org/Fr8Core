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
    public class Create_Contact_v1 : BaseTerminalActivity
    {
        ISalesforceManager _salesforce = new SalesforceManager();

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO,AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            return ConfigurationRequestType.Initial;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO = null)
        {
            using (var updater = Crate.UpdateStorage(curActivityDO))
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

            return await Task.FromResult(curActivityDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            var firstName = ExtractSpecificOrUpstreamValue(curActivityDO, payloadCrates, "firstName");
            var lastName = ExtractSpecificOrUpstreamValue(curActivityDO, payloadCrates, "lastName");
            var mobilePhone = ExtractSpecificOrUpstreamValue(curActivityDO, payloadCrates, "mobilePhone");
            var email = ExtractSpecificOrUpstreamValue(curActivityDO, payloadCrates, "email");
            if (string.IsNullOrEmpty(lastName))
            {
                return Error(payloadCrates, "No last name found in activity.");
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