using System.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.Manifests;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;
using System.Collections.Generic;

namespace terminalSalesforce.Actions
{
    public class Create_Contact_v1 : BaseTerminalActivity
    {
        ISalesforceManager _salesforce = new SalesforceManager();

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO,AuthorizationTokenDO authTokenDO)
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

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO = null)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();

                AddTextSourceControlForDTO<Contact>(
                    crateStorage,
                    "",
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

                //verify the last name has a value provided
                var lastNameControl = configControls.Controls.Single(ctl => ctl.Name.Equals("LastName"));

                if (string.IsNullOrEmpty((lastNameControl as TextSource).ValueSource))
                {
                    lastNameControl.ErrorMessage = "Last Name must be provided for creating Contact.";
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

            var contact = _salesforce.CreateSalesforceDTO<Contact>(curActivityDO, payloadCrates);
            var result = await _salesforce.CreateObject(contact, authTokenDO);

            if (!string.IsNullOrEmpty(result))
            {
                using (var crateStorage = CrateManager.GetUpdatableStorage(payloadCrates))
                {
                    var contactIdFields = new List<FieldDTO> { new FieldDTO("ContactID", result) };
                    crateStorage.Add(Crate.FromContent("Newly Created Salesforce Contact", new StandardPayloadDataCM(contactIdFields)));
                }

                return Success(payloadCrates);
            }

            return Error(payloadCrates, "Contact creation is failed");
        }
    }
}