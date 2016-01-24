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

                AddLeadTextSources<LeadDTO>(updater.CrateStorage);

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

            var firstName = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "FirstName");
            var lastName = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "LastName");
            var company = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "Company");
            var title = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "Title");
            var phone = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "Phone");
            var mobile = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "Mobile");
            var fax = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "Fax");
            var email = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "Email");
            var website = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "Website");
            var street = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "Street");
            var city = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "City");
            var state = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "State");
            var zip = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "Zip");
            var country = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "Country");
            var description = ExtractSpecificOrUpstreamValue(curActionDO, payloadCrates, "Description");

            if (string.IsNullOrEmpty(lastName))
            {
                return Error(payloadCrates, "No last name found in activity.");
            }
            
            if (string.IsNullOrEmpty(company))
            {
                return Error(payloadCrates, "No company name found in activity.");
            }

            var lead = new LeadDTO
            {
                FirstName = firstName,
                LastName = lastName,
                Company = company,
                Title = title,
                Phone = phone,
                Mobile = mobile,
                Fax = fax,
                Email = email,
                Website = website,
                Street = street,
                City = city,
                State = state,
                Zip = zip,
                Country = country,
                Description = description
            };

            bool result = await _salesforce.CreateObject(lead, "Lead", _salesforce.CreateForceClient(authTokenDO));

            if (result)
            {
                return Success(payloadCrates);
            }

            return Error(payloadCrates, "Lead creation is failed");
        }

        private void AddLeadTextSources<T>(CrateStorage crateStorage)
        {
            typeof(T).GetProperties().Where(property => !property.Name.Equals("Id")).ToList().ForEach(
                property => AddTextSourceControl(crateStorage, property.Name, property.Name,
                    "Upstream Terminal-Provided Fields", addRequestConfigEvent: false));
        }
    }
}