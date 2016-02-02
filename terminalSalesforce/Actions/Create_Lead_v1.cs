using System.Linq;
using System.Reflection;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using ServiceStack;
using terminalSalesforce.Infrastructure;
using System.Threading.Tasks;
using Data.Interfaces.Manifests;
using Hub.Managers;
using terminalSalesforce.Services;
using TerminalBase.Infrastructure;
using System.Collections.Generic;
using Data.Entities;
using TerminalBase.BaseClasses;
using System;
using Data.Control;

namespace terminalSalesforce.Actions
{
    public class Create_Lead_v1 : BaseTerminalActivity
    {
        ISalesforceManager _salesforce = new SalesforceManager();

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (Crate.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var storage = Crate.GetStorage(curActivityDO);

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
            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage.Clear();

                AddTextSourceControlForDTO<LeadDTO>(updater.CrateStorage, "Upstream Terminal-Provided Fields");
            }

            return await Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO,
                                                                                AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage.ReplaceByLabel(await CreateAvailableFieldsCrate(curActivityDO));
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

            var lead = _salesforce.CreateSalesforceDTO<LeadDTO>(curActivityDO, payloadCrates, ExtractSpecificOrUpstreamValue);

            if (string.IsNullOrEmpty(lead.LastName))
            {
                return Error(payloadCrates, "No last name found in activity.");
            }

            if (string.IsNullOrEmpty(lead.Company))
            {
                return Error(payloadCrates, "No company name found in activity.");
            }

            bool result = await _salesforce.CreateObject(lead, "Lead", _salesforce.CreateForceClient(authTokenDO));

            if (result)
            {
                return Success(payloadCrates);
            }

            return Error(payloadCrates, "Lead creation is failed");
        }
    }
}