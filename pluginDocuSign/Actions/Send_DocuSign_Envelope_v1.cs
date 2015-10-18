using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Enums;
using Newtonsoft.Json;
using Core.Interfaces;
using Data.Constants;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using DocuSign.Integrations.Client;
using PluginBase.Infrastructure;
using Utilities;
using pluginDocuSign.DataTransferObjects;
using pluginDocuSign.Infrastructure;
using pluginDocuSign.Interfaces;
using pluginDocuSign.Services;
using PluginUtilities.BaseClasses;

namespace pluginDocuSign.Actions
{
    public class Send_DocuSign_Envelope_v1 : BasePluginAction
    {
        public Send_DocuSign_Envelope_v1()
        {
        }

        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            if (IsEmptyAuthToken(curActionDTO))
            {
                AppendDockyardAuthenticationCrate(curActionDTO, AuthenticationMode.InternalMode);
                return curActionDTO;
            }

            RemoveAuthenticationCrate(curActionDTO);

            return await ProcessConfigurationRequest(curActionDTO, actionDTO => ConfigurationEvaluator(actionDTO));
        }

        public object Activate(ActionDTO curActionDTO)
        {
            return "Activate Request"; // Will be changed when implementation is plumbed in.
        }

        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            if (curActionDTO.AuthToken == null)
            {
                throw new ApplicationException("No auth token provided.");
            }

            var processPayload = await GetProcessPayload(curActionDTO.ProcessId);

            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(curActionDTO.AuthToken.Token);

            var curEnvelope = new Envelope();
            curEnvelope.Login = new DocuSignPackager()
                .Login(docuSignAuthDTO.Email, docuSignAuthDTO.ApiPassword);

            curEnvelope = AddTemplateData(curActionDTO, processPayload, curEnvelope);
            curEnvelope.EmailSubject = "Test Message from Fr8";
            curEnvelope.Status = "sent";

            var result = curEnvelope.Create();

            return processPayload;
        }

        private string ExtractTemplateId(ActionDTO curActionDTO)
        {
            var confCrate = curActionDTO.CrateStorage.CrateDTO.FirstOrDefault(
                c => c.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME);

            var controls = Crate.GetStandardConfigurationControls(confCrate).Controls;

            var templateDropDown = controls.SingleOrDefault(x => x.Name == "target_docusign_template");

            if (templateDropDown == null)
            {
                throw new ApplicationException("Could not find target_docusign_template DropDownList control.");
            }

            var result = templateDropDown.Value;
            return result;
        }

        private Envelope AddTemplateData(ActionDTO actionDTO, PayloadDTO processPayload, Envelope curEnvelope)
        {
            var curTemplateId = ExtractTemplateId(actionDTO);
            var curRecipientAddress = ExtractSpecificOrUpstreamValue(
                actionDTO.CrateStorage,
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

        private ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            CrateStorageDTO curCrates = curActionDTO.CrateStorage;
            // Do we have any crate? If no, it means that it's Initial configuration
            if (!curCrates.CrateDTO.Any())
            {
                return ConfigurationRequestType.Initial;
            }

            var curActionDO = AutoMapper.Mapper.Map<ActionDO>(curActionDTO);
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

        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(curActionDTO.AuthToken.Token);

            var template = new DocuSignTemplate();
            template.Login = new DocuSignPackager()
                .Login(docuSignAuthDTO.Email, docuSignAuthDTO.ApiPassword);

            if (curActionDTO.CrateStorage == null)
            {
                curActionDTO.CrateStorage = new CrateStorageDTO();
            }

            // Only do it if no existing MT.StandardDesignTimeFields crate is present to avoid loss of existing settings
            // Two crates are created
            // One to hold the ui controls
            if (!curActionDTO.CrateStorage.CrateDTO.Any(c => c.ManifestId == (int)MT.StandardDesignTimeFields))
            {
                var crateControlsDTO = CreateDocusignTemplateConfigurationControls(curActionDTO);
                // and one to hold the available templates, which need to be requested from docusign
                var crateDesignTimeFieldsDTO = CreateDocusignTemplateNameCrate(template);
                curActionDTO.CrateStorage = AssembleCrateStorage(crateControlsDTO, crateDesignTimeFieldsDTO);
            }

            // Build a crate with the list of available upstream fields
            var curUpstreamFieldsCrate = curActionDTO.CrateStorage.CrateDTO.SingleOrDefault(c =>
                c.ManifestId == (int)MT.StandardDesignTimeFields
                && c.Label == "Upstream Plugin-Provided Fields");

            if (curUpstreamFieldsCrate != null)
            {
                curActionDTO.CrateStorage.CrateDTO.Remove(curUpstreamFieldsCrate);
            }

            var curUpstreamFields = (await GetDesignTimeFields(curActionDTO.Id, GetCrateDirection.Upstream))
                .Fields
                .ToArray();

            curUpstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Upstream Plugin-Provided Fields", curUpstreamFields);
            curActionDTO.CrateStorage.CrateDTO.Add(curUpstreamFieldsCrate);

            return curActionDTO;
        }

        protected override async Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(curActionDTO.AuthToken.Token);
            var curCrates = curActionDTO.CrateStorage.CrateDTO;

            if (curCrates == null || curCrates.Count == 0)
            {
                return curActionDTO;
            }

            var curActionDO = AutoMapper.Mapper.Map<ActionDO>(curActionDTO);
            
            // Try to find Configuration_Controls.
            var stdCfgControlMS = Crate.GetConfigurationControls(curActionDO);
            if (stdCfgControlMS == null)
            {
                return curActionDTO;
            }
            
            // Try to find DocuSignTemplate drop-down.
            var dropdownControlDTO = stdCfgControlMS.FindByName("target_docusign_template");
            if (dropdownControlDTO == null)
            {
                return curActionDTO;
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

            curActionDTO.CrateStorage.CrateDTO.Add(crateUserDefinedDTO);
            curActionDTO.CrateStorage.CrateDTO.Add(crateStandardDTO);

            return await Task.FromResult<ActionDTO>(curActionDTO);
        }

        private CrateDTO CreateDocusignTemplateConfigurationControls(ActionDTO curActionDTO)
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
            
            // var recipientSource = new RadioButtonGroupControlDefinitionDTO()
            // {
            //     Label = "Recipient",
            //     GroupName = "Recipient",
            //     Name = "Recipient",
            //     Radios = new List<RadioButtonOption>()
            //     {
            //         new RadioButtonOption()
            //         {
            //             Selected = true,
            //             Name = "specific",
            //             Value ="This specific value"
            //         },
            //         new RadioButtonOption()
            //         {
            //             Selected = false,
            //             Name = "crate",
            //             Value ="A value from an Upstream Crate"
            //         }
            //     }
            // };
            // 
            // recipientSource.Radios[0].Controls.Add(new TextBoxControlDefinitionDTO()
            // {
            //     Label = "",
            //     Name = "Address"
            // });
            // 
            // recipientSource.Radios[1].Controls.Add(new DropDownListControlDefinitionDTO()
            // {
            //     Label = "",
            //     Name = "Select Upstream Crate",
            //     Source = new FieldSourceDTO
            //     {
            //         Label = "Upstream Plugin-Provided Fields",
            //         ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
            //     }
            // });
            

            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                fieldSelectDocusignTemplateDTO,
                CreateSpecificOrUpstreamValueChooser("Recipient", "Recipient", "Upstream Plugin-Provided Fields")
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