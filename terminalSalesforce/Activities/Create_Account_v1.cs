using System.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;
using Data.Interfaces.Manifests;
using System.Collections.Generic;

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
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var storage = CrateManager.GetStorage(curActivityDO);

            var hasConfigurationControlsCrate = storage
                .CratesOfType<StandardConfigurationControlsCM>(c => c.Label == "Configuration_Controls").FirstOrDefault() != null;

            if (hasConfigurationControlsCrate)
            {
                return ConfigurationRequestType.Followup;
            }

            return ConfigurationRequestType.Initial;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();

                AddTextSourceControlForDTO<Infrastructure.AccountDTO>(
                    crateStorage,
                    "Upstream Terminal-Provided Fields",
                    requestUpstream: true
                );
            }

            return await Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.ReplaceByLabel(await CreateAvailableFieldsCrate(curActivityDO));
            }
            return await Task.FromResult(curActivityDO);
        }

        public override async Task<ActivityDO> Activate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var configControls = GetConfigurationControls(crateStorage);

                //verify the account name has a value provided
                var accountNameControl = configControls.Controls.Single(ctl => ctl.Name.Equals("Name"));

                if (string.IsNullOrEmpty((accountNameControl as TextSource).ValueSource))
                {
                    accountNameControl.ErrorMessage = "Account Name must be provided for creating Account.";
                }
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

            var account = _salesforce.CreateSalesforceDTO<Infrastructure.AccountDTO>(curActivityDO, payloadCrates);
            var result = await _salesforce.CreateObject(account, "Account", authTokenDO);

            if (!string.IsNullOrEmpty(result))
            {
                using (var crateStorage = CrateManager.GetUpdatableStorage(payloadCrates))
                {
                    var accountIdFields = new List<FieldDTO> { new FieldDTO("AccountID", result) };
                    crateStorage.Add(Crate.FromContent("Newly Created Salesforce Account", new StandardPayloadDataCM(accountIdFields)));
                }

                return Success(payloadCrates);
            }

            return Error(payloadCrates, "Account creation is failed");
        }
    }
}