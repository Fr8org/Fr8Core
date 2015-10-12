using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using StructureMap;
using Core.Interfaces;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using PluginBase;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;
using Utilities;
using pluginDocuSign.DataTransferObjects;
using pluginDocuSign.Infrastructure;
using pluginDocuSign.Interfaces;
using pluginDocuSign.Services;

namespace pluginDocuSign.Actions
{
    public class Send_DocuSign_Envelope_v1 : BasePluginAction
    {
 
        // IDocuSignTemplate _template;
        private IDocuSignEnvelope _docusignEnvelope;

        public Send_DocuSign_Envelope_v1()
        {
 
            // _template = ObjectFactory.GetInstance<IDocuSignTemplate>();
            _docusignEnvelope = ObjectFactory.GetInstance<IDocuSignEnvelope>();
        }

        public object Configure(ActionDTO curActionDTO)
        {
            if (IsEmptyAuthToken(curActionDTO))
            {
                AppendDockyardAuthenticationCrate(curActionDTO, AuthenticationMode.InternalMode);
                return curActionDTO;
            }

            RemoveAuthenticationCrate(curActionDTO);

            return ProcessConfigurationRequest(curActionDTO, actionDo => ConfigurationEvaluator(actionDo));
        }

        public object Activate(ActionDTO curActionDTO)
        {
            return "Activate Request"; // Will be changed when implementation is plumbed in.
        }

        public object Execute(ActionDTO curActionDTO)
        {
            CrateDTO confCrate = curActionDTO.CrateStorage.CrateDTO.FirstOrDefault(
                c => c.ManifestId == CrateManifests.STANDARD_CONF_CONTROLS_MANIFEST_ID);

            List<ControlDefinitionDTO> controls = _crate.GetStandardConfigurationControls(confCrate).Controls;
            var recipientSourceControl = controls.SingleOrDefault(c => c.Name == "Recipient") as RadioButtonGroupControlDefinitionDTO;
            string recipientSourceValue = recipientSourceControl.Value;
            string recipientAddress = string.Empty;

            switch (recipientSourceValue)
            {
                case "This specific value":
                    var recipientAddressField = recipientSourceControl.Radios[0].Controls[0];
                    recipientAddress = recipientAddressField.Value;
                    break;

                case "A value from an Upstream Crate":
                    var recipientField = recipientSourceControl.Radios[1].Controls[0];
                    recipientAddress = recipientField.Value;
                    break;
            }

            // User's choosen recipient is in recipientAddress. Someone needs to implement actual submission of the envelope. 
            //_docusignEnvelope.SendUsingTemplate(templateId, recipientAddress);
            ;
            return null;
        }

        private ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            CrateStorageDTO curCrates = curActionDTO.CrateStorage;
            // Do we have any crate? If no, it means that it's Initial configuration
            if (!curCrates.CrateDTO.Any())
                return ConfigurationRequestType.Initial;
            var curActionDO = AutoMapper.Mapper.Map<ActionDO>(curActionDTO);
            // Try to find Configuration_Controls
            var stdCfgControlMS = _action.GetConfigurationControls(curActionDO);
            if (stdCfgControlMS == null)
                return ConfigurationRequestType.Initial;
            // Try to get DropdownListField
            var dropdownControlDTO = stdCfgControlMS.FindByName("target_docusign_template");
            if (dropdownControlDTO == null)
                return ConfigurationRequestType.Initial;

            var docusignTemplateId = dropdownControlDTO.Value;
            if (string.IsNullOrEmpty(docusignTemplateId))
                return ConfigurationRequestType.Initial;

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
            CrateDTO curUpstreamFieldsCrate = curActionDTO.CrateStorage.CrateDTO.SingleOrDefault(c =>
                c.ManifestId == (int)MT.StandardDesignTimeFields
                && c.Label == "Upstream Plugin-Provided Fields");

            if (curUpstreamFieldsCrate != null)
            {
                curActionDTO.CrateStorage.CrateDTO.Remove(curUpstreamFieldsCrate);
            }

            var curUpstreamFields = (await GetDesignTimeFields(curActionDTO.Id, GetCrateDirection.Upstream))
                .Fields
                .ToArray();

            curUpstreamFieldsCrate = _crate.CreateDesignTimeFieldsCrate("Upstream Plugin-Provided Fields", curUpstreamFields);
            curActionDTO.CrateStorage.CrateDTO.Add(curUpstreamFieldsCrate);

            // Execute(curActionDTO); // For testing

            return await Task.FromResult<ActionDTO>(curActionDTO);
        }

        protected override async Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(curActionDTO.AuthToken.Token);
            var curCrates = curActionDTO.CrateStorage.CrateDTO;

            var template = new DocuSignTemplate();
            template.Login = new DocuSignPackager()
                .Login(docuSignAuthDTO.Email, docuSignAuthDTO.ApiPassword);

            if (curCrates == null || curCrates.Count == 0)
                return curActionDTO;
            var curActionDO = AutoMapper.Mapper.Map<ActionDO>(curActionDTO);
            // Try to find Configuration_Controls
            var stdCfgControlMS = _action.GetConfigurationControls(curActionDO);
            if (stdCfgControlMS == null)
                return curActionDTO;
            var dropdownControlDTO = stdCfgControlMS.FindByName("target_docusign_template");
            if (dropdownControlDTO == null)
                return curActionDTO;

            // Get DocuSign Template Id
            var docusignTemplateId = dropdownControlDTO.Value;
            // Get Template
            var docuSignTemplateDTO = template.GetTemplateById(docusignTemplateId);
            var docuSignUserFields = template.GetUserFields(docuSignTemplateDTO);
            //	when we're in design mode, there are no values
            // we just want the names of the fields
            List<FieldDTO> userDefinedFields = new List<FieldDTO>();
            docuSignUserFields.ForEach(x => userDefinedFields.Add(new FieldDTO() { Key = null, Value = x }));
            //  we're in design mode, there are no values 
            List<FieldDTO> standartFields = new List<FieldDTO>() { new FieldDTO() { Key = null, Value = "recipient" } };
            var crateUserDefinedDTO = _crate.CreateDesignTimeFieldsCrate("DocuSignTemplateUserDefinedFields", userDefinedFields.ToArray());
            var crateStandardDTO = _crate.CreateDesignTimeFieldsCrate("DocuSignTemplateStandardFields", standartFields.ToArray());

            curActionDTO.CrateStorage = AssembleCrateStorage(crateUserDefinedDTO, crateStandardDTO);


            return await Task.FromResult<ActionDTO>(curActionDTO);
        }

        private CrateDTO CreateDocusignTemplateConfigurationControls(ActionDTO curActionDTO)
        {
            var fieldSelectDocusignTemplateDTO = new DropDownListControlDefinitionDTO()
            {
                Label = "target_docusign_template",
                Name = "target_docusign_template",
                Required = true,
                Events = new List<ControlEvent>() {
                     new ControlEvent("onSelect", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            };

            var recipientSource = new RadioButtonGroupControlDefinitionDTO()
            {
                Label = "Recipient",
                GroupName = "Recipient",
                Name = "Recipient",
                Radios = new List<RadioButtonOption>()
                {
                    new RadioButtonOption()
                    {
                        Selected = true,
                        Name = "specific",
                        Value ="This specific value"
                    },
                    new RadioButtonOption()
                    {
                        Selected = false,
                        Name = "crate",
                        Value ="A value from an Upstream Crate"
                    }
                }
            };

            recipientSource.Radios[0].Controls.Add(new TextBoxControlDefinitionDTO()
            {
                Label = "",
                Name = "Address"
            });

            recipientSource.Radios[1].Controls.Add(new DropDownListControlDefinitionDTO()
            {
                Label = "",
                Name = "Select Upstream Crate",
                Source = new FieldSourceDTO
                {
                    Label = "Upstream Plugin-Provided Fields",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            });

            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                fieldSelectDocusignTemplateDTO,
                recipientSource
            };
            var controls = new StandardConfigurationControlsMS()
            {
                Controls = fieldsDTO
            };
            return _crate.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }

        private CrateDTO CreateDocusignTemplateNameCrate(IDocuSignTemplate template)
        {
            var templatesDTO = template.GetTemplates(null);
            var fieldsDTO = templatesDTO.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToList();
            var controls = new StandardDesignTimeFieldsMS()
            {
                Fields = fieldsDTO,
            };
            return _crate.CreateDesignTimeFieldsCrate("Available Templates", fieldsDTO.ToArray());
        }
    }
}