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
			if (curCrates.CrateDTO.Count == 0)
				return ConfigurationRequestType.Initial;

			var controlsCrates = _action.GetCratesByManifestType(MT.StandardConfigurationControls.GetEnumDisplayName(), curActionDTO.CrateStorage);
			var curDocuSignTemplateId = _crate.GetElementByKey(controlsCrates, key: "target_docusign_template", keyFieldName: "name")
				 .Select(e => (string)e["value"])
				 .FirstOrDefault(s => !string.IsNullOrEmpty(s));

			if (curDocuSignTemplateId != null)
			{
				return ConfigurationRequestType.Followup;
			}
			else
			{
				return ConfigurationRequestType.Initial;
			}
		}
		protected override ActionDTO InitialConfigurationResponse(ActionDTO curActionDTO)
		{
			if (curActionDTO.CrateStorage == null)
			{
				curActionDTO.CrateStorage = new CrateStorageDTO();
			}
			// Two crates are created
			// One to hold the ui controls
			var crateControls = CreateStandartConfigurationControls();
			// and one to hold the available templates, which need to be requested from docusign
			var crateDesignTimeFields = CreateStandardDesignTimeFields();
			// target_docusign_template
			curActionDTO.CrateStorage.CrateDTO.Add(crateControls);
			curActionDTO.CrateStorage.CrateDTO.Add(crateDesignTimeFields);
			return curActionDTO;
		}
		protected override ActionDTO FollowupConfigurationResponse(ActionDTO curActionDTO)
		{
			var curCrates = curActionDTO.CrateStorage.CrateDTO;

			if (curCrates == null || curCrates.Count == 0)
				return curActionDTO;
			// Try to find Configuration_Controls create which conains Controls with name 'target_docusign_template'
			var configurationFieldsCrate = curCrates.SingleOrDefault(c => c.Label == "Configuration_Controls");
			if (configurationFieldsCrate == null || String.IsNullOrEmpty(configurationFieldsCrate.Contents))
				return curActionDTO;
			
			var configurationFields = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(configurationFieldsCrate.Contents);
			if (configurationFields == null || !configurationFields.Controls.Any(c => c.Name == "target_docusign_template"))
				return curActionDTO;
			
			_crate.RemoveCrateByLabel(curActionDTO.CrateStorage.CrateDTO, "DocuSignTemplateUserDefinedFields");

			// Extract DocuSign Template Id
			var docusignTemplateId = configurationFields.Controls.SingleOrDefault(c => c.Name == "target_docusign_template").Value;
			var userFields = new DocuSignTextTab().GetUserFields(docusignTemplateId);

			//	when we're in design mode, there are no values
			// we just want the names of the fields
			List<FieldDTO> userDefinedFields = new List<FieldDTO>();
			userFields.ForEach(x => userDefinedFields.Add(new FieldDTO() { Key = null, Value = x.name }));
			List<FieldDTO> standartFields = new List<FieldDTO>() { new FieldDTO() { Key = null, Value = "recipient" }};
			//  we're in design mode, there are no values 
			var crateUserDefined = _crate.CreateDesignTimeFieldsCrate("DocuSignTemplateUserDefinedFields", userDefinedFields.ToArray());
			var crateStandart = _crate.CreateDesignTimeFieldsCrate("DocuSignTemplateStandardFields", standartFields.ToArray());
			if (curActionDTO.CrateStorage == null)
			{
				curActionDTO.CrateStorage = new CrateStorageDTO();
			}
			curActionDTO.CrateStorage.CrateDTO.Add(crateUserDefined);
			curActionDTO.CrateStorage.CrateDTO.Add(crateStandart);

			return curActionDTO;
		}
		private CrateDTO CreateStandartConfigurationControls()
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
		private CrateDTO CreateStandardDesignTimeFields()
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