using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using terminalDocuSign.Actions;

namespace terminalDocuSign.Actions
{
    public class Use_DocuSign_Template_With_New_Document_v1 : Send_DocuSign_Envelope_v1
    {

        protected override string ActivityUserFriendlyName => "Use DocuSign Template With New Document";

        protected override PayloadDTO SendAnEnvelope(ICrateStorage curStorage, DocuSignApiConfiguration loginInfo, PayloadDTO payloadCrates,
            List<FieldDTO> rolesList, List<FieldDTO> fieldList, string curTemplateId)
        {
            try
            {
                var fileCrateLabel = (FindControl(curStorage, "document_Override_DDLB") as DropDownList).selectedKey;
                var file_crate = payloadCrates.CrateStorage.Crates.Where(a => a.ManifestType == CrateManifestTypes.StandardFileDescription && a.Label == fileCrateLabel).FirstOrDefault();
                if (file_crate == null)
                {
                    return Error(payloadCrates, $"New document file wasn't found");
                }

                var file_manifest = Crate.FromDto(file_crate).Get<StandardFileDescriptionCM>();
                DocuSignManager.SendAnEnvelopeFromTemplate(loginInfo, rolesList, fieldList, curTemplateId, file_manifest);
            }
            catch (Exception ex)
            {
                return Error(payloadCrates, $"Couldn't send an envelope. {ex}");
            }
            return Success(payloadCrates);
        }

        protected async override Task<Crate> CreateDocusignTemplateConfigurationControls(ActivityDO curActivity)
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

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                curActivityDO = await UpdateFilesDD(curActivityDO, crateStorage);
                await HandleFollowUpConfiguration(curActivityDO, authTokenDO, crateStorage);
            }

            return await Task.FromResult(curActivityDO);
        }

        private async Task<ActivityDO> UpdateFilesDD(ActivityDO curActivityDO, IUpdatableCrateStorage crateStorage)
        {
            var ddlb = (DropDownList)FindControl(crateStorage, "document_Override_DDLB");
            ddlb.ListItems = await GetFilesCrates(curActivityDO);
            return curActivityDO;
        }

        private async Task<List<ListItem>> GetFilesCrates(ActivityDO curActivityDO)
        {
            CrateDescriptionCM cratesDescription = new CrateDescriptionCM();
            var crates = await GetCratesByDirection(curActivityDO, CrateDirection.Upstream);
            var file_crates = crates.Where(a => a.ManifestType.Id == (int)MT.StandardFileHandle);
            if (file_crates.Count() != 0)
                cratesDescription.CrateDescriptions.AddRange(file_crates.Select(a => new CrateDescriptionDTO() { Label = a.Label }));
            var upstream_available_crates = crates.Where(a => a.Label == "Runtime Available Crates").FirstOrDefault();
            if (upstream_available_crates != null)
            {
                cratesDescription.CrateDescriptions.AddRange(upstream_available_crates.Get<CrateDescriptionCM>().CrateDescriptions.Where(a => a.ManifestType == CrateManifestTypes.StandardFileDescription));
            }
            return new List<ListItem>(cratesDescription.CrateDescriptions.Select(a => new ListItem() { Key = a.Label }));
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

            var ddDocument = stdCfgControlMS.FindByName("document_Override_DDLB");
            if (ddDocument == null) { return ConfigurationRequestType.Initial; }

            if (string.IsNullOrEmpty((ddDocument as DropDownList).selectedKey)) { return ConfigurationRequestType.Initial; }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var configurationCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();
                if (configurationCrate == null)
                {
                    configurationCrate = (Crate<StandardConfigurationControlsCM>)(await CreateDocusignTemplateConfigurationControls(curActivityDO));
                    crateStorage.Add(configurationCrate);
                }

                FillDocuSignTemplateSource(configurationCrate, "target_docusign_template", authTokenDO);
                await UpdateFilesDD(curActivityDO, crateStorage);
            }

            return curActivityDO;
        }

        public override Task ValidateActivity(ActivityDO curActivityDO, ICrateStorage crateStorage, ValidationManager validationManager)
        {
            var configControls = GetConfigurationControls(crateStorage);
            if (configControls == null)
            {
                validationManager.SetError(DocuSignValidationUtils.ControlsAreNotConfiguredErrorMessage);
                return Task.FromResult(0);
            }

            var templateList = configControls.Controls.OfType<DropDownList>().FirstOrDefault(a => a.Name == "target_docusign_template");
            var documentsList = configControls.Controls.OfType<DropDownList>().FirstOrDefault(a => a.Name == "document_Override_DDLB");

            if (templateList != null)
            {
                validationManager.ValidateTemplateList(templateList);
            }

            if (!DocuSignValidationUtils.ItemIsSelected(documentsList))
            {
                validationManager.SetError(DocuSignValidationUtils.DocumentIsNotValidErrorMessage, documentsList);
            }

            return Task.FromResult(0);
        }
    }
}