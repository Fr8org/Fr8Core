using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Infrastructure.Behaviors;
using Fr8.TerminalBase.Infrastructure.States;
using terminalDocuSign.Activities;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Actions
{
    public class Send_DocuSign_Envelope_v1 : BaseDocuSignActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("8AC0A48C-C4B5-43E4-B585-2870D814BA86"),
            Version = "1",
            Name = "Send_DocuSign_Envelope",
            Label = "Send DocuSign Envelope",
            Tags = string.Join(",", Tags.EmailDeliverer),
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Forward,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        protected override string ActivityUserFriendlyName => "Send DocuSign Envelope";
        private const string advisoryName = "DocuSign Template Warning";
        private const string advisoryContent = "In your selected template you have fields with default values. Those can be changes inside advanced DocuSign UI to frendlier label.";



        public Send_DocuSign_Envelope_v1(ICrateManager crateManager, IDocuSignManager docuSignManager) 
            : base(crateManager, docuSignManager)
        {
        }

        public override async Task Run()
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
            List<KeyValueDTO> rolesList, List<KeyValueDTO> fieldList, string curTemplateId)
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

        protected List<KeyValueDTO> MapControlsToFields()
        {
            //todo: refactor the method
            var resultCollection = new List<KeyValueDTO>();

            //get existing userDefinedFields 
            var usedDefinedFields = Storage.CrateContentsOfType<KeyValueListCM>(x => x.Label == "DocuSignTemplateUserDefinedFields").FirstOrDefault();
            if (usedDefinedFields != null)
            {
                var tempFieldCollection = usedDefinedFields.Values;

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

        protected List<KeyValueDTO> MapRoleControlsToFields()
        {
            var resultCollection = new List<KeyValueDTO>();

            //get existing userDefinedFields 
            var usedDefinedFields = Storage.CrateContentsOfType<KeyValueListCM>(x => x.Label == "DocuSignTemplateUserDefinedFields").FirstOrDefault();
            if (usedDefinedFields != null)
            {
                var tempFieldCollection = usedDefinedFields.Values;

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

        public override async Task Initialize()
        {
            // Only do it if no existing MT.FieldDescription crate is present to avoid loss of existing settings
            // Two crates are created
            // One to hold the ui controls
            if (Storage.All(c => c.ManifestType.Id != (int) MT.FieldDescription))
            {
                Storage.Clear();

                CreateDocusignTemplateConfigurationControls();

                FillDocuSignTemplateSource("target_docusign_template");
            }
        }

        public override async Task FollowUp()
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
            

            //update docusign templates list to get if new templates were provided by DS
            FillDocuSignTemplateSource("target_docusign_template");
            // Try to find DocuSignTemplate drop-down.
            
            var dropdownControlDTO = ConfigurationControls.FindByName("target_docusign_template");
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

            Storage.RemoveByLabel("DocuSignTemplateRolesFields");
            Storage.Add("DocuSignTemplateRolesFields", new KeyValueListCM(roles));


            var envelopeDataDTO = tabsandfields.Item2;

            var userDefinedFields = tabsandfields.Item1.Where(a => a.Tags.Contains(DocuSignConstants.DocuSignTabTag));

            //check for DocuSign default template names and add advisory json
            var hasDefaultNames = DocuSignManager.DocuSignTemplateDefaultNames(tabsandfields.Item2);
            if (hasDefaultNames)
            {
                var advisoryCrate = Storage.CratesOfType<AdvisoryMessagesCM>().FirstOrDefault();
                var currentAdvisoryResults = advisoryCrate == null ? new AdvisoryMessagesCM() : advisoryCrate.Content;

                var advisory = currentAdvisoryResults.Advisories.FirstOrDefault(x => x.Name == advisoryName);

                if (advisory == null)
                {
                    currentAdvisoryResults.Advisories.Add(new AdvisoryMessageDTO { Name = advisoryName, Content = advisoryContent });
                }
                else
                {
                    advisory.Content = advisoryContent;
                }

                Storage.Add(Crate.FromContent("Advisories", currentAdvisoryResults));
            }
            
            Storage.RemoveByLabel("DocuSignTemplateUserDefinedFields");
            Storage.Add("DocuSignTemplateUserDefinedFields", new KeyValueListCM(userDefinedFields.Concat(roles)));

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

            crateStorage.ReplaceByLabel(Crate.FromContent("ChosenTemplateId", new StandardPayloadDataCM()
            { PayloadObjects = new List<PayloadObjectDTO>() { new PayloadObjectDTO() { PayloadObject = new List<KeyValueDTO>() { new KeyValueDTO("TemplateId", docusignTemplateId) } } } }));

            return docusignTemplateId != previousTemplateId;
        }

        protected override Task Validate()
        {
            if (ConfigurationControls == null)
            {
                ValidationManager.SetError(DocuSignValidationUtils.ControlsAreNotConfiguredErrorMessage);
                return Task.FromResult(0);
            }

            var templateList = ConfigurationControls.Controls.OfType<DropDownList>().FirstOrDefault();
            ValidationManager.ValidateTemplateList(templateList);

            return Task.FromResult(0);
        }

        protected virtual void CreateDocusignTemplateConfigurationControls()
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
            
            AddControls(fieldSelectDocusignTemplateDTO);
        }
    }
}
