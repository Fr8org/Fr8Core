using Data.Entities;
using PluginBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;
using Core.Interfaces;
using StructureMap;
using Newtonsoft.Json;
using Data.Interfaces;
using PluginBase;
using Data.Constants;
using Utilities;
using Data.Interfaces.ManifestSchemas;
using pluginDocuSign.Interfaces;

namespace pluginDocuSign.Actions
{
	public class Send_DocuSign_Envelope_v1 : BasePluginAction
	{
		IDocuSignTemplate _template = ObjectFactory.GetInstance<IDocuSignTemplate>();
		IDocuSignEnvelope _docusignEnvelope = ObjectFactory.GetInstance<IDocuSignEnvelope>();

		public object Configure(ActionDTO curActionDTO)
		{
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
		protected override ActionDTO InitialConfigurationResponse(ActionDTO curActionDTO)
		{
			if (curActionDTO.CrateStorage == null)
			{
				curActionDTO.CrateStorage = new CrateStorageDTO();
			}
			// Two crates are created
			// One to hold the ui controls
			var crateControlsDTO = CreateDocusignTemplateConfigurationControls();
			// and one to hold the available templates, which need to be requested from docusign
			var crateDesignTimeFieldsDTO = CreateDocusignTemplateNameCrate();
			curActionDTO.CrateStorage = AssembleCrateStorage(crateControlsDTO, crateDesignTimeFieldsDTO);
			return curActionDTO;
		}
		protected override ActionDTO FollowupConfigurationResponse(ActionDTO curActionDTO)
		{
			var curCrates = curActionDTO.CrateStorage.CrateDTO;

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
			var docuSignUserFields = _template.GetUserFields(docusignTemplateId);

			//	when we're in design mode, there are no values
			// we just want the names of the fields
			List<FieldDTO> userDefinedFields = new List<FieldDTO>();
			docuSignUserFields.ForEach(x => userDefinedFields.Add(new FieldDTO() { Key = null, Value = x.name }));
			//  we're in design mode, there are no values 
			List<FieldDTO> standartFields = new List<FieldDTO>() { new FieldDTO() { Key = null, Value = "recipient" }};
			var crateUserDefinedDTO = _crate.CreateDesignTimeFieldsCrate("DocuSignTemplateUserDefinedFields", userDefinedFields.ToArray());
			var crateStandardDTO = _crate.CreateDesignTimeFieldsCrate("DocuSignTemplateStandardFields", standartFields.ToArray());

			curActionDTO.CrateStorage = AssembleCrateStorage(crateUserDefinedDTO, crateStandardDTO);

			return curActionDTO;
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

			var fieldsDTO = new List<FieldDefinitionDTO>()
			{
				fieldSelectDocusignTemplateDTO,
			};
			var controls = new StandardConfigurationControlsMS()
			{
				Controls = fieldsDTO
			};
			return _crate.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
		}
		private CrateDTO CreateDocusignTemplateNameCrate()
		{
			var templatesDTO = _template.GetTemplates(null);
			var fieldsDTO = templatesDTO.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToList();
			var controls = new StandardDesignTimeFieldsMS()
			{
				Fields = fieldsDTO,
			};
			return _crate.CreateDesignTimeFieldsCrate("Available Templates", fieldsDTO.ToArray());
		}
	}
}