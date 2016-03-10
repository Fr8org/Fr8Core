using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using TerminalBase.Infrastructure;
using TerminalBase.Infrastructure.Behaviors;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Actions
{
    public class Send_DocuSign_Envelope_v1 : BaseDocuSignActivity
    {
        private DocuSignManager _docuSignManager = new DocuSignManager();

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override string ActivityUserFriendlyName => "Send DocuSign Envelope";

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
            var loginInfo = DocuSignService.Login(docuSignAuthDTO.Email, docuSignAuthDTO.ApiPassword);

            return HandleTemplateData(curActivityDO, loginInfo, payloadCrates);
        }

        private string ExtractTemplateId(ActivityDO curActivityDO)
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

        private PayloadDTO HandleTemplateData(ActivityDO curActivityDO, DocuSignLoginInformation loginInfo, PayloadDTO payloadCrates)
        {
            var curTemplateId = ExtractTemplateId(curActivityDO);
            var payloadCrateStorage = CrateManager.GetStorage(payloadCrates);
            var configurationControls = GetConfigurationControls(curActivityDO);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var fieldList = MapControlsToFields(CrateManager.GetStorage(curActivityDO), payloadCrateStorage);
                var rolesList = MapRoleControlsToFields(CrateManager.GetStorage(curActivityDO), payloadCrateStorage);

                try
                {
                    DocuSignService.SendAnEnvelopeFromTemplate(loginInfo, rolesList, fieldList, curTemplateId);
                }
                //TODO: log this exception
                catch
                {
                    return Error(payloadCrates, "Couldn't send an envelope");
                }
            }

            return Success(payloadCrates);
        }

        private List<FieldDTO> MapControlsToFields(ICrateStorage activityCrateStorage,
            ICrateStorage payloadCrateStorage)
        {
            //todo: refactor the method
            var resultCollection = new List<FieldDTO>();

            //get existing userDefinedFields 
            var usedDefinedFields = activityCrateStorage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "DocuSignTemplateUserDefinedFields").FirstOrDefault();
            if (usedDefinedFields != null)
            {
                var tempFieldCollection = usedDefinedFields.Fields;

                //extract data from text source Controls
                var mappingBehavior = new TextSourceMappingBehavior(activityCrateStorage, "Mapping");
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
                        var selectedIndex = -1;
                        if (selectedItem != null)
                        {
                            selectedIndex = item.Radios.IndexOf(selectedItem);
                        }

                        field.Value = selectedIndex.ToString();
                        resultCollection.Add(field);
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

        private List<FieldDTO> MapRoleControlsToFields(ICrateStorage activityCrateStorage,
            ICrateStorage payloadCrateStorage)
        {
            var resultCollection = new List<FieldDTO>();

            //get existing userDefinedFields 
            var usedDefinedFields = activityCrateStorage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "DocuSignTemplateUserDefinedFields").FirstOrDefault();
            if (usedDefinedFields != null)
            {
                var tempFieldCollection = usedDefinedFields.Fields;

                var mappingBehavior = new TextSourceMappingBehavior(activityCrateStorage, "RolesMapping");
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
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            // Try to find Configuration_Controls
            var stdCfgControlMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            if (stdCfgControlMS == null)
            {
                return ConfigurationRequestType.Initial;
            }

            // Try to get DropdownListField
            var dropdownControlDTO = stdCfgControlMS.FindByName("target_docusign_template");
            if (dropdownControlDTO == null)
            {
                return ConfigurationRequestType.Initial;
            }

            var docusignTemplateId = dropdownControlDTO.Value;
            if (string.IsNullOrEmpty(docusignTemplateId))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                // Only do it if no existing MT.FieldDescription crate is present to avoid loss of existing settings
                // Two crates are created
                // One to hold the ui controls
                if (crateStorage.All(c => c.ManifestType.Id != (int)MT.FieldDescription))
                {
                    var configurationCrate = CreateDocusignTemplateConfigurationControls(curActivityDO);
                    _docuSignManager.FillDocuSignTemplateSource(configurationCrate, "target_docusign_template", docuSignAuthDTO);
                    crateStorage.Replace(new CrateStorage(configurationCrate));
                }

                await UpdateUpstreamCrate(curActivityDO, crateStorage);
            }

            return curActivityDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                if (crateStorage.Count == 0)
                {
                    return curActivityDO;
                }

                await UpdateUpstreamCrate(curActivityDO, crateStorage);

                // Try to find Configuration_Controls.
                var stdCfgControlMS = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
                if (stdCfgControlMS == null)
                {
                    return curActivityDO;
                }

                // Try to find DocuSignTemplate drop-down.
                var dropdownControlDTO = stdCfgControlMS.FindByName("target_docusign_template");
                if (dropdownControlDTO == null)
                {
                    return curActivityDO;
                }

                // Get DocuSign Template Id
                var docusignTemplateId = dropdownControlDTO.Value;

                // Get Template
                var docuSignEnvelope = new DocuSignEnvelope(docuSignAuthDTO.Email, docuSignAuthDTO.ApiPassword);

                var template = docuSignEnvelope.GetTemplateDetails(docusignTemplateId);
                var roles = docuSignEnvelope.GetTemplateRoles(template);
                var crateRolesDTO = CrateManager.CreateDesignTimeFieldsCrate(
                  "DocuSignTemplateRolesFields",
                  roles.ToArray()
              );

                crateStorage.RemoveByLabel("DocuSignTemplateRolesFields");
                crateStorage.Add(crateRolesDTO);

                var envelopeDataDTO = docuSignEnvelope.GetTemplateEnvelopeData(template).ToList();

                // when we're in design mode, there are no values
                // we just want the names of the fields
                var userDefinedFields = envelopeDataDTO.Select(x => new FieldDTO() { Key = x.Name, Value = x.Name, Availability = AvailabilityType.RunTime, Tags = x.TabName + " " + "recipientId:" + x.RecipientId }).ToList();

                var crateUserDefinedDTO = CrateManager.CreateDesignTimeFieldsCrate(
                    "DocuSignTemplateUserDefinedFields",
                    userDefinedFields.Concat(roles).ToArray()
                );
                
                crateStorage.RemoveByLabel("DocuSignTemplateUserDefinedFields");
                crateStorage.Add(crateUserDefinedDTO);

                //Create TextSource controls for ROLES
                var rolesMappingBehavior = new TextSourceMappingBehavior(crateStorage, "RolesMapping");
                rolesMappingBehavior.Clear();
                rolesMappingBehavior.Append(roles.Select(x => x.Key).ToList(), "Upstream Terminal-Provided Fields");

                //Create Text Source controls for TABS
                var textSourceFields = new List<string>();
                textSourceFields = envelopeDataDTO.Where(x => x.Type == ControlTypes.TextBox).Select(x => x.Name).ToList();
                var mappingBehavior = new TextSourceMappingBehavior(
                    crateStorage,
                    "Mapping"
                );
                mappingBehavior.Clear();
                mappingBehavior.Append(textSourceFields, "Upstream Terminal-Provided Fields");
                //Create TextSource controls for ROLES

                //Create radio Button Groups
                var radioButtonGroupBehavior = new RadioButtonGroupMappingBehavior(crateStorage, "RadioGroupMapping");

                radioButtonGroupBehavior.Clear();
                foreach (var item in envelopeDataDTO.Where(x => x.Type == ControlTypes.RadioButtonGroup).ToList())
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
                foreach (var item in envelopeDataDTO.Where(x => x.Type == ControlTypes.CheckBox).ToList())
                {
                    checkBoxMappingBehavior.Append(item.Name,item.Name);
                }

                //create dropdown controls
                var dropdownListMappingBehavior = new DropDownListMappingBehavior(crateStorage, "DropDownMapping");
                dropdownListMappingBehavior.Clear();
                foreach (var item in envelopeDataDTO.Where(x => x.Type == ControlTypes.DropDownList).ToList())
                {
                    var dropDownListDTO = item as DocuSignMultipleOptionsTabDTO;
                    if (dropDownListDTO == null) continue;

                    dropdownListMappingBehavior.Append(dropDownListDTO.Name, string.Format("For the <strong>{0}</strong>, use:", item.Name), dropDownListDTO.Items.Where(x=>x.Text != string.Empty || x.Value != string.Empty ).Select(x => new ListItem()
                    {
                        Key = string.IsNullOrEmpty(x.Value) ? x.Text : x.Value, 
                        Value = string.IsNullOrEmpty(x.Text) ? x.Value : x.Text,
                        Selected = x.Selected,
                    }).ToList());
                }
            }

            return await Task.FromResult(curActivityDO);
        }

        protected internal override bool ActivityIsValid(ActivityDO curActivityDO, out string errorMessage)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var configControls = GetConfigurationControls(crateStorage);
                if (configControls == null)
                {
                    errorMessage = "Controls are not configured properly";
                    return false;
                }
                var templateList = configControls.Controls.OfType<DropDownList>().FirstOrDefault();
                if (templateList?.ListItems.Count == 0)
                {
                    errorMessage = "Please link at least one template to your DocuSign account";
                    return false;
                }
                if (string.IsNullOrEmpty(templateList?.selectedKey))
                {
                    errorMessage = "Template is not selected";
                    return false;
                }
                errorMessage = string.Empty;
                return true;
            }
        }

        private Crate CreateDocusignTemplateConfigurationControls(ActivityDO curActivityDO)
        {
            var fieldSelectDocusignTemplateDTO = new DropDownList()
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

            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                fieldSelectDocusignTemplateDTO
            };

            var controls = new StandardConfigurationControlsCM()
            {
                Controls = fieldsDTO
            };

            return CrateManager.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }

        public async Task UpdateUpstreamCrate(ActivityDO curActivityDO, IUpdatableCrateStorage updater)
        {
            // Build a crate with the list of available upstream fields
            var curUpstreamFieldsCrate = updater.SingleOrDefault(c => c.ManifestType.Id == (int)MT.FieldDescription
                                                                                && c.Label == "Upstream Terminal-Provided Fields");

            if (curUpstreamFieldsCrate != null)
            {
                updater.Remove(curUpstreamFieldsCrate);
            }

            var curUpstreamFields = (await GetDesignTimeFields(curActivityDO.Id, CrateDirection.Upstream))
                .Fields
                .Where(a => a.Availability == AvailabilityType.RunTime)
                .ToArray();

            //make fields inaccessible to up/downstanding actions
            curUpstreamFields.ToList().ForEach(a => a.Availability = AvailabilityType.Configuration);

            curUpstreamFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Upstream Terminal-Provided Fields", curUpstreamFields);
            updater.Add(curUpstreamFieldsCrate);
        }
    }
}
