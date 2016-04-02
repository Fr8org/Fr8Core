using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Entities;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;
using Hub.Managers;
using Data.Control;
using Data.Crates;
using Data.Interfaces.Manifests;
using Data.States;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;

namespace terminalSalesforce.Actions
{
    public class Save_To_SalesforceDotCom_v1 : BaseTerminalActivity
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

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                CreateWhatKindOfDataDDLBControl(crateStorage);
            }

            return await Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var configControlManifest = GetControlsManifest(curActivityDO);
            var whatKindOfDataDDLBControl = (DropDownList)GetControl(configControlManifest, "WhatKindOfData", ControlTypes.DropDownList);

            string curSelectedObject = whatKindOfDataDDLBControl.selectedKey;
            if (string.IsNullOrEmpty(curSelectedObject))
            {
                return await Task.FromResult(curActivityDO);
            }

            var selectedObjectFieldsList = await _salesforce.GetFields(curSelectedObject, authTokenDO, true);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                GetConfigurationControls(crateStorage).Controls.RemoveAll(ctl => ctl is TextSource);
                AddTextSourceControlForListOfFields(selectedObjectFieldsList, crateStorage, string.Empty, requestUpstream: true);

                crateStorage.RemoveByLabel("Salesforce Object Fields");
                crateStorage.Add(CrateManager.CreateDesignTimeFieldsCrate("Salesforce Object Fields", selectedObjectFieldsList.ToList(), AvailabilityType.Configuration));
            }

            return await Task.FromResult(curActivityDO);
        }

        public override async Task<ActivityDO> Activate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var requiredFieldsList = crateStorage
                                            .CrateContentsOfType<FieldDescriptionsCM>(c => c.Label.Equals("Salesforce Object Fields"))
                                            .SelectMany(f => f.Fields.Where(s => s.IsRequired));

                var configControls = GetConfigurationControls(crateStorage);
                var requiredFieldControlsList = configControls.Controls.OfType<TextSource>().Where(c => requiredFieldsList.Any(f => f.Value.Equals(c.Name)));
                var curSelectedObject = configControls.Controls.OfType<DropDownList>().Single(c => c.Name.Equals("WhatKindOfData")).selectedKey;

                requiredFieldControlsList.ToList().ForEach(c =>
                {
                    if (string.IsNullOrEmpty(c.ValueSource))
                    {
                        c.ErrorMessage = string.Format("{0} must be provided for creating {1}", c.Label, curSelectedObject);
                    }
                });
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

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var fieldsList = crateStorage.CrateContentsOfType<FieldDescriptionsCM>(c => c.Label.Equals("Salesforce Object Fields")).SelectMany(f => f.Fields);

                var configControls = GetConfigurationControls(crateStorage);
                var fieldControlsList = configControls.Controls.OfType<TextSource>();
                var curSelectedObject = configControls.Controls.OfType<DropDownList>().Single(c => c.Name.Equals("WhatKindOfData")).selectedKey;

                var jsonInputObject = new Dictionary<string, object>();

                var payloadStorage = CrateManager.FromDto(payloadCrates.CrateStorage);
                fieldsList.ToList().ForEach(field =>
                {
                    var jsonKey = field.Value;
                    var jsonValue = fieldControlsList.Single(ts => ts.Name.Equals(jsonKey)).GetValue(payloadStorage);

                    if (!string.IsNullOrEmpty(jsonValue))
                    {
                        jsonInputObject.Add(jsonKey, jsonValue);
                    }
                });

                var result = await _salesforce.CreateObject<IDictionary<string, object>>(jsonInputObject, curSelectedObject, authTokenDO);

                if (!string.IsNullOrEmpty(result))
                {
                    var contactIdFields = new List<FieldDTO> { new FieldDTO(curSelectedObject + "ID", result) };
                    crateStorage.Add(Crate.FromContent("Newly Created Salesforce " + curSelectedObject, new StandardPayloadDataCM(contactIdFields)));
                    return Success(payloadCrates);
                }

                return Error(payloadCrates, "Saving " + curSelectedObject + " to Salesforce.com is failed.");
            }
        }

        /// <summary>
        /// Clears the storage and adds StandardConfigurationControlsCM crate with only DDLB control named WhatKindOfData
        /// </summary>
        private void CreateWhatKindOfDataDDLBControl(IUpdatableCrateStorage crateStorage)
        {
            crateStorage.Clear();

            //DDLB for What Salesforce Object to be considered
            var whatKindOfData = new DropDownList
            {
                Name = "WhatKindOfData",
                Required = true,
                Label = "Which object do you want to save to Salesforce.com?",
                Source = null,
                Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
            };

            var configurationControls = PackControlsCrate(whatKindOfData);
            ActivitiesHelper.FillSalesforceSupportedObjects(configurationControls, "WhatKindOfData");
            crateStorage.ReplaceByLabel(configurationControls);
        }
    }
}