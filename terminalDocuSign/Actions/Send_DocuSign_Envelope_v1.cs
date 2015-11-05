using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocuSign.Integrations.Client;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Constants;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Enums;
using TerminalBase.Infrastructure;
using Utilities;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;
using TerminalBase.BaseClasses;

namespace terminalDocuSign.Actions
{
    public class Send_DocuSign_Envelope_v1 : BasePluginAction
    {
        public Send_DocuSign_Envelope_v1()
        {
        }

        public async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActionDO, x => ConfigurationEvaluator(x));
        }

        public object Activate(ActionDO curActionDO)
        {
            return "Activate Request"; // Will be changed when implementation is plumbed in.
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, int containerId, AuthorizationTokenDO authTokenDO = null)
        {
            if (authTokenDO == null)
            {
                throw new ApplicationException("No auth token provided.");
            }

            var processPayload = await GetProcessPayload(containerId);

            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(authTokenDO.Token);

            var curEnvelope = new Envelope();
            curEnvelope.Login = new DocuSignPackager()
                .Login(docuSignAuthDTO.Email, docuSignAuthDTO.ApiPassword);

            curEnvelope = AddTemplateData(curActionDO, processPayload, curEnvelope);
            curEnvelope.EmailSubject = "Test Message from Fr8";
            curEnvelope.Status = "sent";

            var result = curEnvelope.Create();

            return processPayload;
        }

        private string ExtractTemplateId(ActionDO curActionDO)
        {
            var confCrate = curActionDO.CrateStorageDTO().CrateDTO.FirstOrDefault(
                c => c.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_MANIFEST_NAME);

            var controls = Crate.GetStandardConfigurationControls(confCrate).Controls;

            var templateDropDown = controls.SingleOrDefault(x => x.Name == "target_docusign_template");

            if (templateDropDown == null)
            {
                throw new ApplicationException("Could not find target_docusign_template DropDownList control.");
            }

            var result = templateDropDown.Value;
            return result;
        }

        private Envelope AddTemplateData(ActionDO actionDO, PayloadDTO processPayload, Envelope curEnvelope)
        {
            var curTemplateId = ExtractTemplateId(actionDO);
            var curRecipientAddress = ExtractSpecificOrUpstreamValue(
                actionDO.CrateStorageDTO(),
                processPayload.CrateStorageDTO(),
                "Recipient"
            );

            curEnvelope.TemplateId = curTemplateId;
            curEnvelope.TemplateRoles = new TemplateRole[]
            {
                new TemplateRole()
                {
                    email = curRecipientAddress,
                    name = curRecipientAddress,
                    roleName = "Signer"   // need to fetch this
                },
            };

            return curEnvelope;
        }

        private ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            CrateStorageDTO curCrates = curActionDO.CrateStorageDTO();
            // Do we have any crate? If no, it means that it's Initial configuration
            if (!curCrates.CrateDTO.Any())
            {
                return ConfigurationRequestType.Initial;
            }


            // Try to find Configuration_Controls
            var stdCfgControlMS = Crate.GetConfigurationControls(curActionDO);
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

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(authTokenDO.Token);

            var template = new DocuSignTemplate();
            template.Login = new DocuSignPackager()
                .Login(docuSignAuthDTO.Email, docuSignAuthDTO.ApiPassword);

            if (curActionDO.CrateStorage == null)
            {
                curActionDO.CrateStorage = "";
            }

            // Only do it if no existing MT.StandardDesignTimeFields crate is present to avoid loss of existing settings
            // Two crates are created
            // One to hold the ui controls
            if (!curActionDO.CrateStorageDTO().CrateDTO.Any(c => c.ManifestId == (int)MT.StandardDesignTimeFields))
            {
                var crateControlsDTO = CreateDocusignTemplateConfigurationControls(curActionDO);
                // and one to hold the available templates, which need to be requested from docusign
                var crateDesignTimeFieldsDTO = CreateDocusignTemplateNameCrate(template);
                curActionDO.UpdateCrateStorageDTO(AssembleCrateStorage(crateControlsDTO, crateDesignTimeFieldsDTO).CrateDTO);
            }

            // Build a crate with the list of available upstream fields
            var curUpstreamFieldsCrate = curActionDO.CrateStorageDTO().CrateDTO.SingleOrDefault(c =>
                c.ManifestId == (int)MT.StandardDesignTimeFields
                && c.Label == "Upstream Plugin-Provided Fields");

            if (curUpstreamFieldsCrate != null)
            {
                curActionDO.CrateStorageDTO().CrateDTO.Remove(curUpstreamFieldsCrate);
            }

            var curUpstreamFields = (await GetDesignTimeFields(curActionDO.Id, GetCrateDirection.Upstream))
                .Fields
                .ToArray();

            curUpstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Upstream Plugin-Provided Fields", curUpstreamFields);
            curActionDO.CrateStorageDTO().CrateDTO.Add(curUpstreamFieldsCrate);

            return curActionDO;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(authTokenDO.Token);
            var curCrates = curActionDO.CrateStorageDTO().CrateDTO;

            if (curCrates == null || curCrates.Count == 0)
            {
                return curActionDO;
            }

            
            // Try to find Configuration_Controls.
            var stdCfgControlMS = Crate.GetConfigurationControls(curActionDO);
            if (stdCfgControlMS == null)
            {
                return curActionDO;
            }
            
            // Try to find DocuSignTemplate drop-down.
            var dropdownControlDTO = stdCfgControlMS.FindByName("target_docusign_template");
            if (dropdownControlDTO == null)
            {
                return curActionDO;
            }

            // Get DocuSign Template Id
            var docusignTemplateId = dropdownControlDTO.Value;
            
            // Get Template
            var docuSignEnvelope = new DocuSignEnvelope(docuSignAuthDTO.Email, docuSignAuthDTO.ApiPassword);
            var envelopeDataDTO = docuSignEnvelope.GetEnvelopeDataByTemplate(docusignTemplateId).ToList();

            // when we're in design mode, there are no values
            // we just want the names of the fields
            var userDefinedFields = new List<FieldDTO>();
            envelopeDataDTO.ForEach(x => userDefinedFields.Add(new FieldDTO() { Key = x.Name, Value = x.Name }));

            // we're in design mode, there are no values 
            var standartFields = new List<FieldDTO>()
            {
                new FieldDTO() { Key = "recipient", Value = "recipient" }
            };
            
            var crateUserDefinedDTO = Crate.CreateDesignTimeFieldsCrate(
                "DocuSignTemplateUserDefinedFields",
                userDefinedFields.ToArray()
            );
            
            var crateStandardDTO = Crate.CreateDesignTimeFieldsCrate(
                "DocuSignTemplateStandardFields",
                standartFields.ToArray()
            );

            curActionDO.CrateStorageDTO().CrateDTO.Add(crateUserDefinedDTO);
            curActionDO.CrateStorageDTO().CrateDTO.Add(crateStandardDTO);

            return await Task.FromResult(curActionDO);
        }

        private CrateDTO CreateDocusignTemplateConfigurationControls(ActionDO curActionDO)
        {
            var fieldSelectDocusignTemplateDTO = new DropDownListControlDefinitionDTO()
            {
                Label = "Use DocuSign Template",
                Name = "target_docusign_template",
                Required = true,
                Events = new List<ControlEvent>()
                {
                     new ControlEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            };
            
            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                fieldSelectDocusignTemplateDTO,
                new TextSourceControlDefinitionDTO("For the Email Address Use", "Upstream Plugin-Provided Fields", "Recipient")
            };

            var controls = new StandardConfigurationControlsCM()
            {
                Controls = fieldsDTO
            };

            return Crate.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }

        private CrateDTO CreateDocusignTemplateNameCrate(IDocuSignTemplate template)
        {
            var templatesDTO = template.GetTemplates(null);
            var fieldsDTO = templatesDTO.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToList();
            var controls = new StandardDesignTimeFieldsCM()
            {
                Fields = fieldsDTO,
            };
            return Crate.CreateDesignTimeFieldsCrate("Available Templates", fieldsDTO.ToArray());
        }
    }
}