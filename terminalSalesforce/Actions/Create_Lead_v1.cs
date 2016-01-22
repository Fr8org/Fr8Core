using System.Linq;
using Data.Interfaces.DataTransferObjects;
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
    public class Create_Lead_v1 : BaseTerminalAction
    {
        ISalesforceManager _salesforce = new SalesforceManager();

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var storage = Crate.GetStorage(curActionDO);

            var hasConfigurationControlsCrate = storage
                .CratesOfType<StandardConfigurationControlsCM>(c => c.Label == "Configuration_Controls").FirstOrDefault() != null;

            if (hasConfigurationControlsCrate)
            {
                return ConfigurationRequestType.Followup;
            }

            return ConfigurationRequestType.Initial;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();

                AddTextSourceControl(updater.CrateStorage, "First Name", "firstName",
                    "Upstream Terminal-Provided Fields", addRequestConfigEvent: false);
                AddTextSourceControl(updater.CrateStorage, "Last Name", "lastName",
                    "Upstream Terminal-Provided Fields", addRequestConfigEvent: false, required:true);
                AddTextSourceControl(updater.CrateStorage, "Company", "companyName",
                    "Upstream Terminal-Provided Fields", addRequestConfigEvent: false, required:true);
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

            var firstName = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates,"firstName");
            var lastName = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "lastName");
            var company = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "companyName");

            if (string.IsNullOrEmpty(lastName))
            {
                return Error(payloadCrates, "No last name found in action.");
            }
            
            if (string.IsNullOrEmpty(company))
            {
                return Error(payloadCrates, "No company name found in action.");
            }

            var lead = new LeadDTO {FirstName = firstName, LastName = lastName, Company = company};

            bool result = await _salesforce.CreateObject(lead, "Lead", _salesforce.CreateForceClient(authTokenDO));

            if (result)
            {
                return Success(payloadCrates);
            }

            return Error(payloadCrates, "Lead creation is failed");
        }
    }
}