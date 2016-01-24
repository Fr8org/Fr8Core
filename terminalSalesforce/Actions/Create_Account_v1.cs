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
    public class Create_Account_v1 : BaseTerminalAction
    {
        ISalesforceManager _salesforce = new SalesforceManager();

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            return ConfigurationRequestType.Initial;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();

                AddTextSourceControl(updater.CrateStorage, "Account Name", "accountName",
                    "Upstream Terminal-Provided Fields", addRequestConfigEvent: false);
                AddTextSourceControl(updater.CrateStorage, "Account Number", "accountNumber",
                    "Upstream Terminal-Provided Fields", addRequestConfigEvent: false, required:true);
                AddTextSourceControl(updater.CrateStorage, "Phone", "phone",
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

            var accountName = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "accountName");
            var accountNumber = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "accountNumber");
            var phone = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "phone");
            if (string.IsNullOrEmpty(accountName))
            {
                return Error(payloadCrates, "No account name found in action.");
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