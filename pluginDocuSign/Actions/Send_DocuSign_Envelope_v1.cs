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
        // TODO: remove this as of DO-1064.
        // IDocuSignTemplate _template;
        // IDocuSignEnvelope _docusignEnvelope;

        public Send_DocuSign_Envelope_v1()
        {
            // TODO: remove this as of DO-1064.
            // _template = ObjectFactory.GetInstance<IDocuSignTemplate>();
            // _docusignEnvelope = ObjectFactory.GetInstance<IDocuSignEnvelope>();
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

        public object Activate(ActionDTO curDataPackage)
        {
            return "Activate Request"; // Will be changed when implementation is plumbed in.
        }

        public object Execute(ActionDataPackageDTO curActionDataPackage)
        {
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
                var crateControlsDTO = CreateDocusignTemplateConfigurationControls();
                // and one to hold the available templates, which need to be requested from docusign
                var crateDesignTimeFieldsDTO = CreateDocusignTemplateNameCrate(template);
                curActionDTO.CrateStorage = AssembleCrateStorage(crateControlsDTO, crateDesignTimeFieldsDTO);
                return await Task.FromResult<ActionDTO>(curActionDTO);
            }
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

        private CrateDTO CreateDocusignTemplateConfigurationControls()
        {
            var fieldSelectDocusignTemplateDTO = new DropdownListFieldDefinitionDTO()
            {
                Label = "target_docusign_template",
                Name = "target_docusign_template",
                Required = true,
                Events = new List<FieldEvent>() {
                     new FieldEvent("onSelect", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            };

            var radioButtonGroup = new RadioButtonGroupFieldDefinitionDTO()
            {
                Label = "Test RadioButtons",
                GroupName = "Group1",
                Name = "Group1",
                Radios = new List<RadioButton>()
                {
                    new RadioButton()
                    {
                        Selected = true,
                        Value ="Test 1"
                    },
                    new RadioButton()
                    {
                        Selected = false,
                        Value ="Test 2"
                    },
                    new RadioButton()
                    {
                        Selected = false,
                        Value ="Test 3"
                    }
                }
            };

            radioButtonGroup.Radios[0].Fields.Add(new TextFieldDefinitionDTO()
            {
                Label = "Test field",
                Name = "Test field"
            });


            var fieldsDTO = new List<ControlsDefinitionDTO>()
			{
				fieldSelectDocusignTemplateDTO,
                radioButtonGroup
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