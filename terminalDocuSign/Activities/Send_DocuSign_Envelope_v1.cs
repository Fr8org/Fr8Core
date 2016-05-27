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
using terminalDocuSign.Activities;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using TerminalBase.Infrastructure;
using TerminalBase.Infrastructure.Behaviors;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Actions
{
    public class Send_DocuSign_Envelope_v1 : BaseDocuSignActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Version = "1",
            Name = "Send_DocuSign_Envelope",
            Label = "Send DocuSign Envelope",
            Category = ActivityCategory.Forwarders,
            Tags = string.Join(",", Tags.EmailDeliverer),
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        protected override string ActivityUserFriendlyName => "Send DocuSign Envelope";

        protected override async Task RunDS()
        {
            var loginInfo = DocuSignManager.SetUp(AuthorizationToken);
            var curTemplateId = ExtractTemplateId();
            var fieldList = MapControlsToFields();
            var rolesList = MapRoleControlsToFields();
            SendAnEnvelope(loginInfo, rolesList, fieldList, curTemplateId);
        }

        protected string ExtractTemplateId()
        {
            var templateDropDown = GetControl<DropDownList>("target_docusign_template");

            if (templateDropDown == null)
            {
                throw new ApplicationException("Could not find target_docusign_template DropDownList control.");
            }
            var result = templateDropDown.Value;
            return result;
        }

        protected virtual void SendAnEnvelope(DocuSignApiConfiguration loginInfo,
            List<FieldDTO> rolesList, List<FieldDTO> fieldList, string curTemplateId)
        {
            try
            {
                DocuSignManager.SendAnEnvelopeFromTemplate(loginInfo, rolesList, fieldList, curTemplateId);
            }
            catch (Exception ex)
            {
                RaiseError($"Couldn't send an envelope. {ex}");
                return;
            }
            Success();
        }

        protected List<FieldDTO> MapControlsToFields()
        {
            //todo: refactor the method
            var resultCollection = new List<FieldDTO>();

            //get existing userDefinedFields 
            var usedDefinedFields = Storage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "DocuSignTemplateUserDefinedFields").FirstOrDefault();
            if (usedDefinedFields != null)
            {
                var tempFieldCollection = usedDefinedFields.Fields;

                //extract data from text source Controls
                var mappingBehavior = new TextSourceMappingBehavior(Storage, "Mapping", true);
                var textSourceValues = mappingBehavior.GetValues(Payload);
                foreach (var item in textSourceValues)
                {
                    var field = tempFieldCollection.FirstOrDefault(x => x.Key == item.Key);
                    if (field != null)
                    {
                        field.Value = item.Value;
                        resultCollection.Add(field);
                    }
                }

                var radiopGroupMappingBehavior = new RadioButtonGroupMappingBehavior(Storage, "RadioGroupMapping");
                var radioButtonGroups = radiopGroupMappingBehavior.GetValues(Payload);
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

                var checkBoxMappingBehavior = new CheckBoxMappingBehavior(Storage, "CheckBoxMapping");
                var checkboxes = checkBoxMappingBehavior.GetValues(Payload);
                foreach (var item in checkboxes)
                {
                    var field = tempFieldCollection.FirstOrDefault(x => x.Key == item.Name);
                    if (field != null)
                    {
                        field.Value = item.Selected.ToString().ToLower();
                        resultCollection.Add(field);
                    }
                }

                var dropdownListMappingBehavior = new DropDownListMappingBehavior(Storage, "DropDownMapping");
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

        protected List<FieldDTO> MapRoleControlsToFields()
        {
            var resultCollection = new List<FieldDTO>();

            //get existing userDefinedFields 
            var usedDefinedFields = Storage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "DocuSignTemplateUserDefinedFields").FirstOrDefault();
            if (usedDefinedFields != null)
            {
                var tempFieldCollection = usedDefinedFields.Fields;

                var mappingBehavior = new TextSourceMappingBehavior(Storage, "RolesMapping", true);
                var textSourceValues = mappingBehavior.GetValues(Payload);
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


        protected override ConfigurationRequestType GetConfigurationRequestType()
        {
            // Do we have any crate? If no, it means that it's Initial configuration
            if (Storage.Count < 1) { return ConfigurationRequestType.Initial; }
            // Try to find Configuration_Controls
            if (ConfigurationControls == null) { return ConfigurationRequestType.Initial; }
            // Try to get DropdownListField
            var dropdownControlDTO = GetControl<DropDownList>("target_docusign_template");
            var docusignTemplateId = dropdownControlDTO?.Value;
            if (string.IsNullOrEmpty(docusignTemplateId)) { return ConfigurationRequestType.Initial; }
            return ConfigurationRequestType.Followup;
        }

        protected override async Task InitializeDS()
        {
                // Only do it if no existing MT.FieldDescription crate is present to avoid loss of existing settings
                // Two crates are created
                // One to hold the ui controls
            if (Storage.All(c => c.ManifestType.Id != (int)MT.FieldDescription))
                {
                var configurationCrate = await CreateDocusignTemplateConfigurationControls();
                FillDocuSignTemplateSource(configurationCrate, "target_docusign_template");
                Storage.Clear();
                Storage.Add(configurationCrate);
            }
        }

        protected override async Task FollowUpDS()
            {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(AuthorizationToken.Token);
            await HandleFollowUpConfiguration();
        }

        protected async Task HandleFollowUpConfiguration()
        {
            if (Storage.Count == 0)
            {
                return;
            }

            // Try to find Configuration_Controls.
            var stdCfgControlCrate = Storage.CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            if (stdCfgControlCrate == null)
            {
                return;
            }

            //update docusign templates list to get if new templates were provided by DS
            FillDocuSignTemplateSource(stdCfgControlCrate, "target_docusign_template");
            // Try to find DocuSignTemplate drop-down.
            var stdCfgControlMS = stdCfgControlCrate.Get<StandardConfigurationControlsCM>();
            var dropdownControlDTO = stdCfgControlMS.FindByName("target_docusign_template");
            if (dropdownControlDTO == null)
            {
                return;
            }
            // Get DocuSign Template Id
            var docusignTemplateId = dropdownControlDTO.Value;
            //Abort configuration if templateId is the same that before
            if (!IsNewTemplateIdChoosen(Storage, docusignTemplateId))
                return;

            var conf = DocuSignManager.SetUp(AuthorizationToken);
            var tabsandfields = DocuSignManager.GetTemplateRecipientsTabsAndDocuSignTabs(conf, docusignTemplateId);

            var roles = tabsandfields.Item1.Where(a => a.Tags.Contains(DocuSignConstants.DocuSignSignerTag));
            var crateRolesDTO = CrateManager.CreateDesignTimeFieldsCrate(
              "DocuSignTemplateRolesFields",
              AvailabilityType.Configuration,
              roles.ToArray()

          );

            Storage.RemoveByLabel("DocuSignTemplateRolesFields");
            Storage.Add(crateRolesDTO);


            var envelopeDataDTO = tabsandfields.Item2;

            var userDefinedFields = tabsandfields.Item1.Where(a => a.Tags.Contains(DocuSignConstants.DocuSignTabTag));

            var crateUserDefinedDTO = CrateManager.CreateDesignTimeFieldsCrate(
                "DocuSignTemplateUserDefinedFields",
                AvailabilityType.Configuration,
                userDefinedFields.Concat(roles).ToArray()
            );

            Storage.RemoveByLabel("DocuSignTemplateUserDefinedFields");
            Storage.Add(crateUserDefinedDTO);

            //Create TextSource controls for ROLES
            var rolesMappingBehavior = new TextSourceMappingBehavior(Storage, "RolesMapping", true);
            rolesMappingBehavior.Clear();
            rolesMappingBehavior.Append(roles.Select(x => x.Key).ToList(), "Upstream Terminal-Provided Fields", AvailabilityType.RunTime);

            //Create Text Source controls for TABS
            var textSourceFields = new List<string>();
            textSourceFields = envelopeDataDTO.Where(x => x.Fr8DisplayType == ControlTypes.TextBox).Select(x => x.Name).ToList();
            var mappingBehavior = new TextSourceMappingBehavior(
                Storage,
                "Mapping",
                true
            );
            mappingBehavior.Clear();
            mappingBehavior.Append(textSourceFields, "Upstream Terminal-Provided Fields", AvailabilityType.RunTime);
            //Create TextSource controls for ROLES

            //Create radio Button Groups
            var radioButtonGroupBehavior = new RadioButtonGroupMappingBehavior(Storage, "RadioGroupMapping");

            radioButtonGroupBehavior.Clear();
            foreach (var item in envelopeDataDTO.Where(x => x.Fr8DisplayType == ControlTypes.RadioButtonGroup).ToList())
            {
                var radioButtonGroupDTO = item as DocuSignMultipleOptionsTabDTO;
                if (radioButtonGroupDTO == null) continue;
                //todo: migrate the string format for label into template
                radioButtonGroupBehavior.Append(radioButtonGroupDTO.Name,
                    $"For the <strong>{radioButtonGroupDTO.Name}</strong>, use:", radioButtonGroupDTO.Items.Select(x => new RadioButtonOption()
                {
                    Name = x.Value,
                    Value = x.Value,
                    Selected = x.Selected
                }).ToList());
            }

            //create checkbox controls
            var checkBoxMappingBehavior = new CheckBoxMappingBehavior(Storage, "CheckBoxMapping");
            checkBoxMappingBehavior.Clear();
            foreach (var item in envelopeDataDTO.Where(x => x.Fr8DisplayType == ControlTypes.CheckBox).ToList())
            {
                checkBoxMappingBehavior.Append(item.Name, item.Name);
            }

            //create dropdown controls
            var dropdownListMappingBehavior = new DropDownListMappingBehavior(Storage, "DropDownMapping");
            dropdownListMappingBehavior.Clear();
            foreach (var item in envelopeDataDTO.Where(x => x.Fr8DisplayType == ControlTypes.DropDownList).ToList())
            {
                var dropDownListDTO = item as DocuSignMultipleOptionsTabDTO;
                if (dropDownListDTO == null) continue;

                dropdownListMappingBehavior.Append(dropDownListDTO.Name, $"For the <strong>{item.Name}</strong>, use:", dropDownListDTO.Items.Where(x => x.Text != string.Empty || x.Value != string.Empty).Select(x => new ListItem()
                {
                    Key = string.IsNullOrEmpty(x.Value) ? x.Text : x.Value,
                    Value = string.IsNullOrEmpty(x.Text) ? x.Value : x.Text,
                    Selected = x.Selected,
                }).ToList());
            }
        }

        protected bool IsNewTemplateIdChoosen(ICrateStorage crateStorage, string docusignTemplateId)
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

        protected override Task<bool> Validate()
        {
            if (ConfigurationControls == null)
            {
                ValidationManager.SetError(DocuSignValidationUtils.ControlsAreNotConfiguredErrorMessage);
                return Task.FromResult(false);
            }
            var templateList = ConfigurationControls.Controls.OfType<DropDownList>().FirstOrDefault();
            ValidationManager.ValidateTemplateList(templateList);
            return Task.FromResult(true);
        }

        protected virtual async Task<Crate> CreateDocusignTemplateConfigurationControls()
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
