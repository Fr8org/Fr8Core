using System.Linq;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using terminalSalesforce.Infrastructure;
using System.Threading.Tasks;
using Data.Interfaces.Manifests;
using Hub.Managers;
using terminalSalesforce.Services;
using TerminalBase.Infrastructure;
using Data.Entities;
using TerminalBase.BaseClasses;
using System;
using Data.Control;
using System.Collections.Generic;

namespace terminalSalesforce.Actions
{
    public class Create_Lead_v1 : BaseTerminalActivity
    {
        ISalesforceManager _salesforce = new SalesforceManager();

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return curActivityDO;
            }

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

                AddTextSourceControlForDTO<LeadDTO>(
                    crateStorage,
                    "",
                    addRequestConfigEvent: true,
                    requestUpstream: true
                );
            }

            return await Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO,
                                                                                AuthorizationTokenDO authTokenDO)
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

                //verify the last name has a value provided
                var lastNameControl = configControls.Controls.Single(ctl => ctl.Name.Equals("LastName"));

                if (string.IsNullOrEmpty((lastNameControl as TextSource).ValueSource))
                {
                    lastNameControl.ErrorMessage = "Last Name must be provided for creating Lead.";
                }

                //verify the company has a value provided
                var companyControl = configControls.Controls.Single(ctl => ctl.Name.Equals("Company")); 

                if (string.IsNullOrEmpty((companyControl as TextSource).ValueSource))
                {
                    companyControl.ErrorMessage = "Company must be provided for creating Lead.";
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

            var lead = _salesforce.CreateSalesforceDTO<LeadDTO>(curActivityDO, payloadCrates);
            var result = await _salesforce.CreateObject(lead, "Lead", authTokenDO);

            if (!string.IsNullOrEmpty(result))
            {
                using (var crateStorage = CrateManager.GetUpdatableStorage(payloadCrates))
                {
                    var leadIdFields = new List<FieldDTO> { new FieldDTO("LeadID", result) };
                    crateStorage.Add(Crate.FromContent("Newly Created Salesforce Lead", new StandardPayloadDataCM(leadIdFields)));
                }

                return Success(payloadCrates);
            }

            return Error(payloadCrates, "Lead creation is failed");
        }
    }
}