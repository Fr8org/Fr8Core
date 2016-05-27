using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap.Diagnostics;
using terminalDocuSign.Actions;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services.New_Api;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Activities
{
    public class Use_DocuSign_Template_With_New_Document_v1 : Send_DocuSign_Envelope_v1
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Version = "1",
            Name = "Use_DocuSign_Template_With_New_Document",
            Label = "Use DocuSign Template With New Document",
            Category = ActivityCategory.Forwarders,
            Tags = Tags.EmailDeliverer,
            NeedsAuthentication = true,
            MinPaneWidth = 380,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        protected override string ActivityUserFriendlyName => "Use DocuSign Template With New Document";

        protected override void SendAnEnvelope(DocuSignApiConfiguration loginInfo,
            List<FieldDTO> rolesList, List<FieldDTO> fieldList, string curTemplateId)
        {
            try
            {
                var fileCrateLabel = GetControl<DropDownList>("document_Override_DDLB").selectedKey;
                var file_crate = Payload.FirstOrDefault(a => a.ManifestType.ToString() == CrateManifestTypes.StandardFileDescription && a.Label == fileCrateLabel);
                if (file_crate == null)
                {
                    RaiseError($"New document file wasn't found");
                    return;
                }

                var file_manifest = file_crate.Get<StandardFileDescriptionCM>();
                DocuSignManager.SendAnEnvelopeFromTemplate(loginInfo, rolesList, fieldList, curTemplateId, file_manifest);
            }
            catch (Exception ex)
            {
                RaiseError($"Couldn't send an envelope. {ex}");
            }
            Success();
        }

        protected override async Task<Crate> CreateDocusignTemplateConfigurationControls()
        {
            var infoBox = new TextBlock() { Value = @"This Activity overlays the tabs from an existing Template onto a new Document and sends out a DocuSign Envelope. 
                                                        When this Activity executes, it will look for and expect to be provided from upstream with one Excel or Word file." };

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

            var documentOverrideDDLB = new DropDownList
            {
                Label = "Use new document",
                Name = "document_Override_DDLB",
                Events = new List<ControlEvent>()
                {
                     ControlEvent.RequestConfig
                },
                Required = true
            };

            var fieldsDTO = new List<ControlDefinitionDTO>
            {
                infoBox, documentOverrideDDLB, fieldSelectDocusignTemplateDTO
            };

            var controls = new StandardConfigurationControlsCM
            {
                Controls = fieldsDTO
            };

            return CrateManager.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }

        protected override async Task FollowUpDS()
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(AuthorizationToken.Token);
            await UpdateFilesDD();
            await HandleFollowUpConfiguration();
        }

        private async Task UpdateFilesDD()
        {
            var ddlb = GetControl<DropDownList>("document_Override_DDLB");
            ddlb.ListItems = await GetFilesCrates();
        }

        private async Task<List<ListItem>> GetFilesCrates()
        {
            CrateDescriptionCM cratesDescription = new CrateDescriptionCM();
            var crates = await GetCratesByDirection(CrateDirection.Upstream);
            var file_crates = crates.Where(a => a.ManifestType.Id == (int)MT.StandardFileHandle);
            if (file_crates.Count() != 0)
                cratesDescription.CrateDescriptions.AddRange(file_crates.Select(a => new CrateDescriptionDTO() { Label = a.Label }));
            var upstream_available_crates = crates.FirstOrDefault(a => a.Label == "Runtime Available Crates");
            if (upstream_available_crates != null)
            {
                cratesDescription.CrateDescriptions.AddRange(upstream_available_crates.Get<CrateDescriptionCM>().CrateDescriptions.Where(a => a.ManifestType == CrateManifestTypes.StandardFileDescription));
            }
            return new List<ListItem>(cratesDescription.CrateDescriptions.Select(a => new ListItem() { Key = a.Label }));
        }

        protected virtual ConfigurationRequestType GetConfigurationRequestType()
        {
            // Do we have any crate? If no, it means that it's Initial configuration
            if (Storage.Count < 1) { return ConfigurationRequestType.Initial; }
            // Try to find Configuration_Controls
            var stdCfgControlMS = Storage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            // Try to get DropdownListField
            var dropdownControlDTO = stdCfgControlMS?.FindByName("target_docusign_template");
            var docusignTemplateId = dropdownControlDTO?.Value;
            if (string.IsNullOrEmpty(docusignTemplateId)) { return ConfigurationRequestType.Initial; }
            var ddDocument = stdCfgControlMS.FindByName("document_Override_DDLB");
            if (string.IsNullOrEmpty((ddDocument as DropDownList)?.selectedKey)) { return ConfigurationRequestType.Initial; }
            return ConfigurationRequestType.Followup;
        }

        protected override async Task InitializeDS()
        {
            var configurationCrate = Storage.CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            if (configurationCrate == null)
            {
                configurationCrate = (Crate<StandardConfigurationControlsCM>)(await CreateDocusignTemplateConfigurationControls());
                Storage.Add(configurationCrate);
            }
            FillDocuSignTemplateSource(configurationCrate, "target_docusign_template");
            await UpdateFilesDD();
        }

        protected override Task<bool> Validate()
        {
            if (ConfigurationControls == null)
            {
                ValidationManager.SetError(DocuSignValidationUtils.ControlsAreNotConfiguredErrorMessage);
                return Task.FromResult(false);
            }

            var templateList = GetControl<DropDownList>("target_docusign_template");
            var documentsList = GetControl<DropDownList>("document_Override_DDLB");
            if (templateList != null)
            {
                ValidationManager.ValidateTemplateList(templateList);
            }
            if (!DocuSignValidationUtils.ItemIsSelected(documentsList))
            {
                ValidationManager.SetError(DocuSignValidationUtils.DocumentIsNotValidErrorMessage, documentsList);
            }
            return Task.FromResult(true);
        }
    }
}