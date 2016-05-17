using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Data.Entities;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Managers;
using terminalDocuSign.DataTransferObjects;
using TerminalBase.Infrastructure;
using TerminalBase.Infrastructure.Behaviors;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Actions
{
    public class Send_DocuSign_Envelope_v1 : BaseDocuSignActivity
    {

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return curActivityDO;
            }

            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override string ActivityUserFriendlyName => "Send DocuSign Envelope";

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);
            var loginInfo = DocuSignManager.SetUp(authTokenDO);
            var curTemplateId = ExtractTemplateId(curActivityDO);
            var payloadCrateStorage = CrateManager.GetStorage(payloadCrates);
            var fieldList = MapControlsToFields(CrateManager.GetStorage(curActivityDO), payloadCrateStorage);
            var rolesList = MapRoleControlsToFields(CrateManager.GetStorage(curActivityDO), payloadCrateStorage);

            return SendAnEnvelope(CrateManager.GetStorage(curActivityDO), loginInfo, payloadCrates, rolesList, fieldList, curTemplateId);
        }

        protected string ExtractTemplateId(ActivityDO curActivityDO)
        {
            var controls = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().First().Controls;

            var templateDropDown = controls.SingleOrDefault(x => x.Name == "target_docusign_template");

            if (templateDropDown == null)
            {
                throw new ApplicationException("Could not find target_docusign_template DropDownList control.");
            }

            var result = templateDropDown.Value;
            return result;
        }

        protected virtual PayloadDTO SendAnEnvelope(ICrateStorage curStorage, DocuSignApiConfiguration loginInfo, PayloadDTO payloadCrates,
            List<FieldDTO> rolesList, List<FieldDTO> fieldList, string curTemplateId)
        {
            try
            {
                DocuSignManager.SendAnEnvelopeFromTemplate(loginInfo, rolesList, fieldList, curTemplateId);
            }
            catch (Exception ex)
            {
                return Error(payloadCrates, $"Couldn't send an envelope. {ex}");
            }
            return Success(payloadCrates);
        }

        protected List<FieldDTO> MapControlsToFields(ICrateStorage activityCrateStorage, ICrateStorage payloadCrateStorage)
        {
            //todo: refactor the method
            var resultCollection = new List<FieldDTO>();

            //get existing userDefinedFields 
            var usedDefinedFields = activityCrateStorage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "DocuSignTemplateUserDefinedFields").FirstOrDefault();
            if (usedDefinedFields != null)
            {
                var tempFieldCollection = usedDefinedFields.Fields;

                //extract data from text source Controls
                var mappingBehavior = new TextSourceMappingBehavior(activityCrateStorage, "Mapping", true);
                var textSourceValues = mappingBehavior.GetValues(payloadCrateStorage);
                foreach (var item in textSourceValues)
                {
                    var field = tempFieldCollection.FirstOrDefault(x => x.Key == item.Key);
                    if (field != null)
                    {
                        field.Value = item.Value;
                        resultCollection.Add(field);
                    }
                }

                var radiopGroupMappingBehavior = new RadioButtonGroupMappingBehavior(activityCrateStorage, "RadioGroupMapping");
                var radioButtonGroups = radiopGroupMappingBehavior.GetValues(payloadCrateStorage);
                foreach (var item in radioButtonGroups)
                {
                    var field = tempFieldCollection.FirstOrDefault(x => x.Key == item.GroupName);
                    if (field != null)
                    {
                        //get index of selected value 
                        var selectedItem = item.Radios.FirstOrDefault(x => x.Selected);
                        if (selectedItem != null)
                        {
                            field.Value = selectedItem.Value.ToString();
                            resultCollection.Add(field);
                        }
                    }
                }

                var checkBoxMappingBehavior = new CheckBoxMappingBehavior(activityCrateStorage, "CheckBoxMapping");
                var checkboxes = checkBoxMappingBehavior.GetValues(payloadCrateStorage);
                foreach (var item in checkboxes)
                {
                    var field = tempFieldCollection.FirstOrDefault(x => x.Key == item.Name);
                    if (field != null)
                    {
                        field.Value = item.Selected.ToString().ToLower();
                        resultCollection.Add(field);
                    }
                }

                var dropdownListMappingBehavior = new DropDownListMappingBehavior(activityCrateStorage, "DropDownMapping");
                var dropDownLists = dropdownListMappingBehavior.GetValues();
                foreach (var item in dropDownLists)
                {
                    var field = tempFieldCollection.FirstOrDefault(x => x.Key == item.Name);
                    if (field != null)
                    {
                        field.Value = item.selectedKey;
                        resultCollection.Add(field);
                    }
                }

            }

            return resultCollection;
        }

        protected List<FieldDTO> MapRoleControlsToFields(ICrateStorage activityCrateStorage, ICrateStorage payloadCrateStorage)
        {
            var resultCollection = new List<FieldDTO>();

            //get existing userDefinedFields 
            var usedDefinedFields = activityCrateStorage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "DocuSignTemplateUserDefinedFields").FirstOrDefault();
            if (usedDefinedFields != null)
            {
                var tempFieldCollection = usedDefinedFields.Fields;

                var mappingBehavior = new TextSourceMappingBehavior(activityCrateStorage, "RolesMapping", true);
                var textSourceValues = mappingBehavior.GetValues(payloadCrateStorage);
                foreach (var item in textSourceValues)
                {
                    var field = tempFieldCollection.FirstOrDefault(x => x.Key == item.Key);
                    if (field != null)
                    {
                        //field.Tags = "RecepientId:" + item.Value;
                        field.Value = item.Value;
                        resultCollection.Add(field);
                    }
                }
            }
            return resultCollection;
        }


        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            // Do we have any crate? If no, it means that it's Initial configuration
            if (CrateManager.IsStorageEmpty(curActivityDO)) { return ConfigurationRequestType.Initial; }

            // Try to find Configuration_Controls
            var stdCfgControlMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            if (stdCfgControlMS == null) { return ConfigurationRequestType.Initial; }

            // Try to get DropdownListField
            var dropdownControlDTO = stdCfgControlMS.FindByName("target_docusign_template");
            if (dropdownControlDTO == null) { return ConfigurationRequestType.Initial; }

            var docusignTemplateId = dropdownControlDTO.Value;
            if (string.IsNullOrEmpty(docusignTemplateId)) { return ConfigurationRequestType.Initial; }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                // Only do it if no existing MT.FieldDescription crate is present to avoid loss of existing settings
                // Two crates are created
                // One to hold the ui controls
                if (crateStorage.All(c => c.ManifestType.Id != (int)MT.FieldDescription))
                {
                    var configurationCrate = await CreateDocusignTemplateConfigurationControls(curActivityDO);
                    FillDocuSignTemplateSource(configurationCrate, "target_docusign_template", authTokenDO);
                    crateStorage.Replace(new CrateStorage(configurationCrate));
                }
            }

            return curActivityDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                curActivityDO = await HandleFollowUpConfiguration(curActivityDO, authTokenDO, crateStorage);
            }

            return await Task.FromResult(curActivityDO);
        }

        protected async Task<ActivityDO> HandleFollowUpConfiguration(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO, IUpdatableCrateStorage crateStorage)
        {
            if (crateStorage.Count == 0)
            {
                return curActivityDO;
            }

            // Try to find Configuration_Controls.
            var stdCfgControlCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            if (stdCfgControlCrate == null)
            {
                return curActivityDO;
            }

            //update docusign templates list to get if new templates were provided by DS
            FillDocuSignTemplateSource(stdCfgControlCrate, "target_docusign_template", authTokenDO);

            // Try to find DocuSignTemplate drop-down.
            var stdCfgControlMS = stdCfgControlCrate.Get<StandardConfigurationControlsCM>();
            var dropdownControlDTO = stdCfgControlMS.FindByName("target_docusign_template");
            if (dropdownControlDTO == null)
            {
                return curActivityDO;
            }

            // Get DocuSign Template Id
            var docusignTemplateId = dropdownControlDTO.Value;

            //Abort configuration if templateId is the same that before
            if (!IsNewTemplateIdChoosen(crateStorage, docusignTemplateId))
                return curActivityDO;


            var conf = DocuSignManager.SetUp(authTokenDO);

            var tabsandfields = DocuSignManager.GetTemplateRecipientsTabsAndDocuSignTabs(conf, docusignTemplateId);

            var roles = tabsandfields.Item1.Where(a => a.Tags.Contains("DocuSigner"));
            var crateRolesDTO = CrateManager.CreateDesignTimeFieldsCrate(
              "DocuSignTemplateRolesFields",
              AvailabilityType.Configuration,
              roles.ToArray()
            );

            crateStorage.RemoveByLabel("DocuSignTemplateRolesFields");
            crateStorage.Add(crateRolesDTO);


            var envelopeDataDTO = tabsandfields.Item2;

            var userDefinedFields = tabsandfields.Item1.Where(a => a.Tags.Contains("DocuSignTab"));

            //check for DocuSign default template names and add advisory json
            var hasDefaultNames = DocuSignManager.DocuSignTemplateDefaultNames(userDefinedFields);

            var crateUserDefinedDTO = CrateManager.CreateDesignTimeFieldsCrate(
                "DocuSignTemplateUserDefinedFields",
                AvailabilityType.Configuration,
                userDefinedFields.Concat(roles).ToArray()
            );

            crateStorage.RemoveByLabel("DocuSignTemplateUserDefinedFields");
            crateStorage.Add(crateUserDefinedDTO);

            //Create TextSource controls for ROLES
            var rolesMappingBehavior = new TextSourceMappingBehavior(crateStorage, "RolesMapping", true);
            rolesMappingBehavior.Clear();
            rolesMappingBehavior.Append(roles.Select(x => x.Key).ToList(), "Upstream Terminal-Provided Fields", AvailabilityType.RunTime);

            //Create Text Source controls for TABS
            var textSourceFields = new List<string>();
            textSourceFields = envelopeDataDTO.Where(x => x.Fr8DisplayType == ControlTypes.TextBox).Select(x => x.Name).ToList();
            var mappingBehavior = new TextSourceMappingBehavior(
                crateStorage,
                "Mapping",
                true
            );
            mappingBehavior.Clear();
            mappingBehavior.Append(textSourceFields, "Upstream Terminal-Provided Fields", AvailabilityType.RunTime);
            //Create TextSource controls for ROLES

            //Create radio Button Groups
            var radioButtonGroupBehavior = new RadioButtonGroupMappingBehavior(crateStorage, "RadioGroupMapping");

            radioButtonGroupBehavior.Clear();
            foreach (var item in envelopeDataDTO.Where(x => x.Fr8DisplayType == ControlTypes.RadioButtonGroup).ToList())
            {
                var radioButtonGroupDTO = item as DocuSignMultipleOptionsTabDTO;
                if (radioButtonGroupDTO == null) continue;
                //todo: migrate the string format for label into template
                radioButtonGroupBehavior.Append(radioButtonGroupDTO.Name, string.Format("For the <strong>{0}</strong>, use:", radioButtonGroupDTO.Name), radioButtonGroupDTO.Items.Select(x => new RadioButtonOption()
                {
                    Name = x.Value,
                    Value = x.Value,
                    Selected = x.Selected
                }).ToList());
            }

            //create checkbox controls
            var checkBoxMappingBehavior = new CheckBoxMappingBehavior(crateStorage, "CheckBoxMapping");
            checkBoxMappingBehavior.Clear();
            foreach (var item in envelopeDataDTO.Where(x => x.Fr8DisplayType == ControlTypes.CheckBox).ToList())
            {
                checkBoxMappingBehavior.Append(item.Name, item.Name);
            }

            //create dropdown controls
            var dropdownListMappingBehavior = new DropDownListMappingBehavior(crateStorage, "DropDownMapping");
            dropdownListMappingBehavior.Clear();
            foreach (var item in envelopeDataDTO.Where(x => x.Fr8DisplayType == ControlTypes.DropDownList).ToList())
            {
                var dropDownListDTO = item as DocuSignMultipleOptionsTabDTO;
                if (dropDownListDTO == null) continue;

                dropdownListMappingBehavior.Append(dropDownListDTO.Name, string.Format("For the <strong>{0}</strong>, use:", item.Name), dropDownListDTO.Items.Where(x => x.Text != string.Empty || x.Value != string.Empty).Select(x => new ListItem()
                {
                    Key = string.IsNullOrEmpty(x.Value) ? x.Text : x.Value,
                    Value = string.IsNullOrEmpty(x.Text) ? x.Value : x.Text,
                    Selected = x.Selected,
                }).ToList());
            }

            return curActivityDO;
        }

        protected bool IsNewTemplateIdChoosen(IUpdatableCrateStorage crateStorage, string docusignTemplateId)
        {
            // Get previous DocuSign Template Id
            string previousTemplateId = "";
            var previousTemplateIdCrate = crateStorage.FirstCrateOrDefault<StandardPayloadDataCM>(a => a.Label == "ChosenTemplateId");
            if (previousTemplateIdCrate != null)
            {
                previousTemplateId = previousTemplateIdCrate.Get<StandardPayloadDataCM>().GetValueOrDefault("TemplateId");
            }

            crateStorage.ReplaceByLabel(Fr8Data.Crates.Crate.FromContent("ChosenTemplateId", new StandardPayloadDataCM()
            { PayloadObjects = new List<PayloadObjectDTO>() { new PayloadObjectDTO() { PayloadObject = new List<FieldDTO>() { new FieldDTO("TemplateId", docusignTemplateId) } } } }));

            return docusignTemplateId != previousTemplateId;
        }

        public override Task ValidateActivity(ActivityDO curActivityDO, ICrateStorage crateStorage, ValidationManager validationManager)
        {
            var configControls = GetConfigurationControls(crateStorage);
            if (configControls == null)
            {
                validationManager.SetError(DocuSignValidationUtils.ControlsAreNotConfiguredErrorMessage);
                return Task.FromResult(0);
            }

            var templateList = configControls.Controls.OfType<DropDownList>().FirstOrDefault();

            validationManager.ValidateTemplateList(templateList);

            return Task.FromResult(0);
        }

        protected async virtual Task<Crate> CreateDocusignTemplateConfigurationControls(ActivityDO curActivity)
        {
            var fieldSelectDocusignTemplateDTO = new DropDownList
            {
                Label = "Use DocuSign Template",
                Name = "target_docusign_template",
                Required = true,
                Events = new List<ControlEvent>()
                {
                     ControlEvent.RequestConfig
                },
                Source = null
            };

            var fieldsDTO = new List<ControlDefinitionDTO>
            {
                fieldSelectDocusignTemplateDTO
            };
            
            return CrateManager.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }
    }
}
