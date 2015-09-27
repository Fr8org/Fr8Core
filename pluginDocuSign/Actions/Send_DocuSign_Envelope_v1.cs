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
using Data.Wrappers;
using Data.Interfaces;
using PluginBase;
using Data.Constants;
using Utilities;
using Data.Interfaces.ManifestSchemas;

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

			// Try to find Configuration_Controls
			var stdCfgControl = _action.GetConfigurationControls(curActionDTO);
			if (stdCfgControl == null)
				return ConfigurationRequestType.Initial;
			// Try to get DropdownListField
			var dropdownControl = stdCfgControl.FindByName("target_docusign_template");
			if (dropdownControl == null)
				return ConfigurationRequestType.Initial;

			var docusignTemplateId = dropdownControl.Value;
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
			var crateControls = CreateDocusignTemplateConfigurationControls();
			// and one to hold the available templates, which need to be requested from docusign
			var crateDesignTimeFields = CreateDocusignTemplateNameCrate();
			curActionDTO.CrateStorage = AssembleCrateStorage(crateControls, crateDesignTimeFields);
			return curActionDTO;
		}
		protected override ActionDTO FollowupConfigurationResponse(ActionDTO curActionDTO)
		{
			var curCrates = curActionDTO.CrateStorage.CrateDTO;

			if (curCrates == null || curCrates.Count == 0)
				return curActionDTO;
			// Try to find Configuration_Controls
			var stdCfgControl = _action.GetConfigurationControls(curActionDTO);
			if (stdCfgControl == null)
				return curActionDTO;
			var dropdownControl = stdCfgControl.FindByName("target_docusign_template");
			if (dropdownControl == null)
				return curActionDTO;
			
			// Get DocuSign Template Id
			var docusignTemplateId = dropdownControl.Value;
			var docuSignUserFields = _template.GetUserFields(docusignTemplateId);

			//	when we're in design mode, there are no values
			// we just want the names of the fields
			List<FieldDTO> userDefinedFields = new List<FieldDTO>();
			docuSignUserFields.ForEach(x => userDefinedFields.Add(new FieldDTO() { Key = null, Value = x.name }));
			//  we're in design mode, there are no values 
			List<FieldDTO> standartFields = new List<FieldDTO>() { new FieldDTO() { Key = null, Value = "recipient" }};
			var crateUserDefined = _crate.CreateDesignTimeFieldsCrate("DocuSignTemplateUserDefinedFields", userDefinedFields.ToArray());
			var crateStandard = _crate.CreateDesignTimeFieldsCrate("DocuSignTemplateStandardFields", standartFields.ToArray());

			curActionDTO.CrateStorage = AssembleCrateStorage(crateUserDefined, crateStandard);

			return curActionDTO;
		}
		private CrateDTO CreateDocusignTemplateConfigurationControls()
		{
			var fieldSelectDocusignTemplate = new DropdownListFieldDefinitionDTO()
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

			var fields = new List<FieldDefinitionDTO>()
			{
				fieldSelectDocusignTemplate,
			};
			var controls = new StandardConfigurationControlsMS()
			{
				Controls = fields
			};
			return _crate.CreateStandardConfigurationControlsCrate("Configuration_Controls", fields.ToArray());
		}
		private CrateDTO CreateDocusignTemplateNameCrate()
		{
			var templates = _template.GetTemplates(null);
			var fields = templates.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToList();
			var controls = new StandardDesignTimeFieldsMS()
			{
				Fields = fields,
			};
			return _crate.CreateDesignTimeFieldsCrate("Available Templates", fields.ToArray());
		}
	}
}