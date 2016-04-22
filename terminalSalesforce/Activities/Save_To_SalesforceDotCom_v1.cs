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
    /// <summary>
    /// A general activity which is used to save any Salesforce object dynamically.
    /// </summary>
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
            //if storage is empty, return initial config
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
                //In initial config, just create a DDLB 
                //to let the user select which object they want to save.
                CreateInitialControls(crateStorage);
            }

            return await Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //In Follow Up config, Get Fields of the user selected object(ex., Lead) and populate Text Source controls
            //to let the user to specify the values.

            //if user did not select any object, retur the activity as it is
            string chosenObject = ExtractChosenSFObject(curActivityDO);
            if (string.IsNullOrEmpty(chosenObject))
            {
                return await Task.FromResult(curActivityDO);
            }

            if(CrateManager.GetStorage(curActivityDO).CratesOfType<FieldDescriptionsCM>().Any(x => x.Label.EndsWith(" - " + chosenObject)))
            {
                return await Task.FromResult(curActivityDO);
            }
                

            var chosenObjectFieldsList = await _salesforce.GetFields(chosenObject, authTokenDO, true);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                //clear any existing TextSources. This is required when user changes the object in DDLB
                GetConfigurationControls(crateStorage).Controls.RemoveAll(ctl => ctl is TextSource);
                chosenObjectFieldsList.ToList().ForEach(selectedObjectField => 
                    AddTextSourceControl(crateStorage, selectedObjectField.Value, selectedObjectField.Key, string.Empty, requestUpstream: true));

                //create design time fields for the downstream activities.
                crateStorage.RemoveByLabelPrefix("Salesforce Object Fields - ");
                crateStorage.Add(CrateManager.CreateDesignTimeFieldsCrate("Salesforce Object Fields - " + chosenObject, 
                                                                                chosenObjectFieldsList.ToList(), AvailabilityType.Configuration));
            }

            return await Task.FromResult(curActivityDO);
        }

        public override async Task<ActivityDO> Activate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                //In Activate, we validate whether the user specified values for the Required controls

                var chosenObject = ExtractChosenSFObject(curActivityDO);

                //get Fields which are reqired
                var requiredFieldsList = GetRequiredFields(curActivityDO, "Salesforce Object Fields - " + chosenObject);

                //get TextSources that represent the above required fields
                var requiredFieldControlsList = GetConfigurationControls(crateStorage)
                                                    .Controls.OfType<TextSource>()
                                                    .Where(c => requiredFieldsList.Any(f => f.Key.Equals(c.Name)));
                
                //for each required field's control, check its value source
                requiredFieldControlsList.ToList().ForEach(c =>
                {
                    if (string.IsNullOrEmpty(c.ValueSource))
                    {
                        c.ErrorMessage = string.Format("{0} must be provided for creating {1}", c.Label, chosenObject);
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
                var chosenObject = ExtractChosenSFObject(curActivityDO);

                //get all fields
                var fieldsList = crateStorage.CrateContentsOfType<FieldDescriptionsCM>(c => c.Label.Equals("Salesforce Object Fields - " + chosenObject))
                                             .SelectMany(f => f.Fields);

                //get all text sources
                var fieldControlsList = GetConfigurationControls(crateStorage).Controls.OfType<TextSource>();
                
                var payloadStorage = CrateManager.FromDto(payloadCrates.CrateStorage);

                //get <Field> <Value> key value pair for the non empty field
                var jsonInputObject = ActivitiesHelper.GenerateSalesforceObjectDictionary(fieldsList, fieldControlsList, payloadStorage);

                var result = await _salesforce.CreateObject(jsonInputObject, chosenObject, authTokenDO);

                if (!string.IsNullOrEmpty(result))
                {
                    using (var paylodCrateStroage = CrateManager.GetUpdatableStorage(payloadCrates))
                    {
                        var contactIdFields = new List<FieldDTO> { new FieldDTO(chosenObject + "ID", result) };
                        paylodCrateStroage.Add(Crate.FromContent(chosenObject + " is saved in Salesforce.com", new StandardPayloadDataCM(contactIdFields)));
                        return Success(payloadCrates);
                    }
                }

                return Error(payloadCrates, "Saving " + chosenObject + " to Salesforce.com is failed.");
            }
        }

        /// <summary>
        /// Creates Initial config controls
        /// </summary>
        private void CreateInitialControls(IUpdatableCrateStorage crateStorage)
        {
            AddSFObjectChooserControl(crateStorage);
        }

        /// <summary>
        /// Clears the storage and adds StandardConfigurationControlsCM crate with only DDLB control named sfObjectType
        /// </summary>
        private void AddSFObjectChooserControl(IUpdatableCrateStorage crateStorage)
        {
            crateStorage.Clear();

            //DDLB for What Salesforce Object to be considered
            var whatKindOfData = new DropDownList
            {
                Name = "sfObjectType",
                Required = true,
                Label = "Which object do you want to save to Salesforce.com?",
                Source = null,
                Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
            };

            var configurationControls = PackControlsCrate(whatKindOfData);
            ActivitiesHelper.GetAvailableFields(configurationControls, "sfObjectType");
            crateStorage.ReplaceByLabel(configurationControls);
        }

        /// <summary>
        /// Extracts current selected SF Object by the user
        /// </summary>
        /// <param name="curActivityDO"></param>
        /// <returns></returns>
        private string ExtractChosenSFObject(ActivityDO curActivityDO)
        {
            var configControls = GetConfigurationControls(curActivityDO);
            var curChosenSFObject = configControls.Controls.OfType<DropDownList>().Single(c => c.Name.Equals("sfObjectType")).selectedKey;

            return curChosenSFObject;
        }
    }
}