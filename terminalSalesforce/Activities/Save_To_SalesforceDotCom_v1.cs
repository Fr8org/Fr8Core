using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Infrastructure;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;
using ServiceStack;

namespace terminalSalesforce.Actions
{
    /// <summary>
    /// A general activity which is used to save any Salesforce object dynamically.
    /// </summary>
    public class Save_To_SalesforceDotCom_v1 : BaseTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Version = "1",
            Name = "Save_To_SalesforceDotCom",
            Label = "Save to Salesforce.Com",
            NeedsAuthentication = true,
            Category = ActivityCategory.Forwarders,
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        readonly ISalesforceManager _salesforce = new SalesforceManager();
        
        public Save_To_SalesforceDotCom_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }


        public override async Task Initialize()
        {
                //In initial config, just create a DDLB 
                //to let the user select which object they want to save.
            CreateInitialControls(Storage);
        }

        public override async Task FollowUp()
        {
            //In Follow Up config, Get Fields of the user selected object(ex., Lead) and populate Text Source controls
            //to let the user to specify the values.

            //if user did not select any object, retur the activity as it is
            string chosenObject = ExtractChosenSFObject();
            if (string.IsNullOrEmpty(chosenObject))
            {
                return;
            }

            if (Storage.CratesOfType<FieldDescriptionsCM>().Any(x => x.Label.EndsWith(" - " + chosenObject)))
            {
                return;
            }

            var chosenObjectFieldsList = await _salesforce.GetProperties(chosenObject.ToEnum<SalesforceObjectType>(), AuthorizationToken, true);

            //clear any existing TextSources. This is required when user changes the object in DDLB
            ConfigurationControls.Controls.RemoveAll(ctl => ctl is TextSource);
            chosenObjectFieldsList.ToList().ForEach(selectedObjectField =>
                AddControl(ControlHelper.CreateTextSourceControl(selectedObjectField.Value, selectedObjectField.Key, string.Empty, addRequestConfigEvent: true, requestUpstream: true)));

            //create design time fields for the downstream activities.
            Storage.RemoveByLabelPrefix("Salesforce Object Fields - ");
            Storage.Add(CrateManager.CreateDesignTimeFieldsCrate("Salesforce Object Fields - " + chosenObject,
                                                                                chosenObjectFieldsList.ToList(), AvailabilityType.Configuration));
        }

        protected override Task Validate()
        {
            var chosenObject = ExtractChosenSFObject();

            //get Fields which are reqired
            var requiredFieldsList = GetRequiredFields("Salesforce Object Fields - " + chosenObject);

            //get TextSources that represent the above required fields
            var requiredFieldControlsList = ConfigurationControls
                                                .Controls.OfType<TextSource>()
                                                .Where(c => requiredFieldsList.Any(f => f.Key.Equals(c.Name)));

            //for each required field's control, check its value source
            requiredFieldControlsList.ToList().ForEach(c =>
            {
                if (!c.HasValue || (c.CanGetValue(ValidationManager.Payload) && string.IsNullOrWhiteSpace(c.GetValue(ValidationManager.Payload))))
                {
                    ValidationManager.SetError($"{c.Label} must be provided for creating {chosenObject}", c);
                }
            });

            var controls = ConfigurationControls.Controls.Where(c => c.Name.Contains("Phone") || c.Name == "Fax");
            foreach (var control in controls)
            {
                var ctrl = (TextSource)control;
                if (ctrl != null)
                {
                    if (ctrl.TextValue != null)
                    {                        
                        ValidationManager.ValidatePhoneNumber(ctrl.TextValue, ctrl);
                    }
                }
            }
            return Task.FromResult(0);
        }

        public override async Task Run()
            {
            using (var validationScope = new RuntimeValidationScope(this, Payload))
            {
                await Validate();
                if (validationScope.HasErrors)
                {
                    // errors will be written during validationScope disposal
                    return;
                }

                var chosenObject = ExtractChosenSFObject();

                //get all fields
                var fieldsList = Storage.CrateContentsOfType<FieldDescriptionsCM>(c => c.Label.Equals("Salesforce Object Fields - " + chosenObject))
                    .SelectMany(f => f.Fields);

                //get all text sources
                var fieldControlsList = ConfigurationControls.Controls.OfType<TextSource>();

                //get <Field> <Value> key value pair for the non empty field
                var jsonInputObject = ActivitiesHelper.GenerateSalesforceObjectDictionary(fieldsList, fieldControlsList, Payload);

                string result;

                try
                {
                    result = await _salesforce.Create(chosenObject.ToEnum<SalesforceObjectType>(), jsonInputObject, AuthorizationToken);
                }
                catch (AuthorizationTokenExpiredOrInvalidException ex)
                {
                    RaiseInvalidTokenError();
                    return;
                }

                if (!string.IsNullOrEmpty(result))
                {
                    var contactIdFields = new List<FieldDTO> { new FieldDTO(chosenObject + "ID", result) };
                    Payload.Add(Crate.FromContent(chosenObject + " is saved in Salesforce.com", new StandardPayloadDataCM(contactIdFields)));
                    Success();
                    return;
                }

                RaiseError("Saving " + chosenObject + " to Salesforce.com is failed.");
                return;
            }
        }
    
        /// <summary>
        /// Creates Initial config controls
        /// </summary>
        private void CreateInitialControls(ICrateStorage crateStorage)
        {
            AddSFObjectChooserControl(crateStorage);
        }

        /// <summary>
        /// Clears the storage and adds StandardConfigurationControlsCM crate with only DDLB control named sfObjectType
        /// </summary>
        private void AddSFObjectChooserControl(ICrateStorage crateStorage)
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
        private string ExtractChosenSFObject()
        {
            var curChosenSFObject = GetControl<DropDownList>("sfObjectType").selectedKey;
            return curChosenSFObject;
        }

        protected override bool IsInvalidTokenException(Exception ex)
        {
            return SalesforceAuthHelper.IsTokenInvalidation(ex);
        }
    }
}