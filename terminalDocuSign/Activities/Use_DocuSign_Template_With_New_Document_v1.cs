using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fr8.Infrastructure.Data.Constants;
using fr8.Infrastructure.Data.Control;
using fr8.Infrastructure.Data.Crates;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.Manifests;
using fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Infrastructure;
using terminalDocuSign.Actions;
using terminalDocuSign.Services.New_Api;

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

        public Use_DocuSign_Template_With_New_Document_v1(ICrateManager crateManager, IDocuSignManager docuSignManager) 
            : base(crateManager, docuSignManager)
        {
        }


        protected override void SendAnEnvelope(DocuSignApiConfiguration loginInfo,
            List<FieldDTO> rolesList, List<FieldDTO> fieldList, string curTemplateId)
        {
            try
            {
                var documentSelector = GetControl<CrateChooser>("document_selector");

                var fileCrate = documentSelector.GetValue(Payload);
                if (fileCrate == null)
                {
                    RaiseError($"New document file wasn't found");
                    return;
                }

                var file_manifest = fileCrate.Get<StandardFileDescriptionCM>();
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

            var documentSelector = new CrateChooser
            {
                Label = "Use new document",
                Name = "document_selector",
                Events = new List<ControlEvent>
                {
                    ControlEvent.RequestConfig
                },
                Required = true
            };

            var fieldsDTO = new List<ControlDefinitionDTO>
            {
                infoBox, documentSelector, fieldSelectDocusignTemplateDTO
            };

            return CrateManager.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }

        protected override async Task FollowUpDS()
        {
            await HandleFollowUpConfiguration();
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
        }

        protected override Task Validate()
        {
            if (ConfigurationControls == null)
            {
                ValidationManager.SetError(DocuSignValidationUtils.ControlsAreNotConfiguredErrorMessage);
                return Task.FromResult(0);
            }

            var templateList = GetControl<DropDownList>("target_docusign_template");
            var documentSelector = GetControl<CrateChooser>("document_selector");

            if (templateList != null)
            {
                ValidationManager.ValidateTemplateList(templateList);
            }

            if (ValidationManager.ValidateCrateChooserNotEmpty(documentSelector, DocuSignValidationUtils.DocumentIsNotValidErrorMessage))
            {
                var selectedCrate = documentSelector.CrateDescriptions.First(x => x.Selected);

                if (selectedCrate.ManifestId != (int) MT.FieldDescription)
                {
                    ValidationManager.SetError("Only File Description crates are allowed.", documentSelector);
                }
            }
            
            return Task.FromResult(0);
        }
    }
}