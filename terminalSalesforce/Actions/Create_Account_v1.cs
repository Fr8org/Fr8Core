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
    public class Create_Account_v1 : BaseTerminalActivity
    {
        ISalesforceManager _salesforce = new SalesforceManager();

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            return ConfigurationRequestType.Initial;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage.Clear();

                AddTextSourceControl(updater.CrateStorage, "Account Name", "accountName",
                    "Upstream Terminal-Provided Fields", addRequestConfigEvent: false);
                AddTextSourceControl(updater.CrateStorage, "Account Number", "accountNumber",
                    "Upstream Terminal-Provided Fields", addRequestConfigEvent: false, required:true);
                AddTextSourceControl(updater.CrateStorage, "Phone", "phone",
                    "Upstream Terminal-Provided Fields", addRequestConfigEvent: false, required:true);
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

            var accountName = ExtractSpecificOrUpstreamValue(curActivityDO, payloadCrates, "accountName");
            var accountNumber = ExtractSpecificOrUpstreamValue(curActivityDO, payloadCrates, "accountNumber");
            var phone = ExtractSpecificOrUpstreamValue(curActivityDO, payloadCrates, "phone");
            if (string.IsNullOrEmpty(accountName))
            {
                return Error(payloadCrates, "No account name found in activity.");
            }
            var account = new AccountDTO {AccountNumber = accountNumber, Name = accountName, Phone = phone};

            bool result = await _salesforce.CreateObject(account, "Account", _salesforce.CreateForceClient(authTokenDO));

            if (result)
            {
                return Success(payloadCrates);
            }

            return Error(payloadCrates, "Account creation is failed");
        }
    }
}