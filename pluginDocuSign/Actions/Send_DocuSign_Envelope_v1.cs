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

		IAction _action = ObjectFactory.GetInstance<IAction>();
		ICrate _crate = ObjectFactory.GetInstance<ICrate>();
		IDocuSignTemplate _template = ObjectFactory.GetInstance<IDocuSignTemplate>();
		IEnvelope _docusignEnvelope = ObjectFactory.GetInstance<IEnvelope>();


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
			// Extract envelope id from the payload Crate
			string envelopeId = GetEnvelopeId(curActionDataPackage.PayloadDTO);

			// Make sure that it exists
			if (String.IsNullOrEmpty(envelopeId))
				throw new PluginCodedException(PluginErrorCode.PAYLOAD_DATA_MISSING, "EnvelopeId");

			////Create a field
			//var fields = new List<FieldDTO>()
			//{
			//    new FieldDTO()
			//    {
			//        Key = "EnvelopeId",
			//        Value = envelopeId
			//    }
			//};

			//var cratePayload = _crate.Create("DocuSign Envelope Payload Data", JsonConvert.SerializeObject(fields), STANDARD_PAYLOAD_MANIFEST_NAME, STANDARD_PAYLOAD_MANIFEST_ID);
			//curActionDataPackage.ActionDTO.CrateStorage.CratesDTO.Add(cratePayload);

			return null;
		}
		private ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
		{
			CrateStorageDTO curCrates = curActionDTO.CrateStorage;

			if (curCrates.CrateDTO.Count == 0)
				return ConfigurationRequestType.Initial;

			//load configuration crates of manifest type Standard Control Crates
			//look for a text field name connection string with a value
			var controlsCrates = _action.GetCratesByManifestType(ManifestIdEnum.StandardConfigurationControls.GetEnumDisplayName(), curActionDTO.CrateStorage);
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
		private string GetEnvelopeId(PayloadDTO curPayloadDTO)
		{
			var crate = curPayloadDTO.CrateStorageDTO().CrateDTO.SingleOrDefault();
			if (crate == null) return null;

			var fields = JsonConvert.DeserializeObject<List<FieldDTO>>(crate.Contents);
			if (fields == null || fields.Count == 0) return null;

			var envelopeIdField = fields.SingleOrDefault(f => f.Key == "EnvelopeId");
			if (envelopeIdField == null) return null;

			return envelopeIdField.Value;
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
			{
				return curActionDTO;
			}

			// Extract DocuSign Template Id
			var configurationFieldsCrate = curCrates.SingleOrDefault(c => c.Label == "Configuration_Controls");
			if (configurationFieldsCrate == null || String.IsNullOrEmpty(configurationFieldsCrate.Contents))
			{
				return curActionDTO;
			}

			var configurationFields = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(configurationFieldsCrate.Contents);
			if (configurationFields == null || !configurationFields.Controls.Any(c => c.Name == "target_docusign_template"))
			{
				return curActionDTO;
			}

			_crate.RemoveCrateByLabel(curActionDTO.CrateStorage.CrateDTO, "DocuSignTemplateUserDefinedFields");

			var docusignTemplateId = configurationFields.Controls.SingleOrDefault(c => c.Name == "target_docusign_template").Value;
			DocuSignTemplate docusignTemplate = new DocuSignTemplate();
			var jObjTemplate = docusignTemplate.GetTemplate(docusignTemplateId);
			var recipientsToken = jObjTemplate.SelectToken("recipients");
			if (recipientsToken == null)
				return curActionDTO;
			var singersToken = recipientsToken.SelectToken("signers");
			var agentsToken = recipientsToken.SelectToken("agents");
			var editorsToken = recipientsToken.SelectToken("editors");
			var intermediariesToken = recipientsToken.SelectToken("intermediaries");
			var carbonCopiesToken = recipientsToken.SelectToken("carbonCopies");
			var certifiedDeliveriesToken = recipientsToken.SelectToken("certifiedDeliveries");
			var inPersonSignersToken = recipientsToken.SelectToken("inPersonSigners");

			// TODO How to send all these data to client?
			List<PayloadMappingsDTO> singers = GetDocusingRoleValues(singersToken);
			List<PayloadMappingsDTO> agents = GetDocusingRoleValues(agentsToken);
			List<PayloadMappingsDTO> editors = GetDocusingRoleValues(editorsToken);
			List<PayloadMappingsDTO> intermediaries = GetDocusingRoleValues(intermediariesToken);
			List<PayloadMappingsDTO> carbonCopies = GetDocusingRoleValues(carbonCopiesToken);
			List<PayloadMappingsDTO> certifiedDeliveries = GetDocusingRoleValues(certifiedDeliveriesToken);
			List<PayloadMappingsDTO> inPersonSigners = GetDocusingRoleValues(inPersonSignersToken);

			CrateDTO singersCrate = null;
			
			//if (singers.Count != 0)
			//	singersCrate = _crate.CreateDesignTimeFieldsCrate("Singer Fields", singers);




			var userDefinedFields = _template.GetTemplates(null).Where(x => x.Id == docusignTemplateId).FirstOrDefault();
			var crateConfiguration = new List<CrateDTO>();
			var fieldCollection = new List<FieldDTO>();
			//var crateContentsObject = new StandardPayloadDataMS()
			//{
			//	Fields = new List<FieldDTO>(fieldCollection)
			//};

			//crateConfiguration.Add(_crate.Create(
			//	 "DocuSignTemplateUserDefinedFields",
			//	 JsonConvert.SerializeObject(crateContentsObject),
			//	 DESIGNTIME_FIELDS_MANIFEST_NAME,
			//	 DESIGNTIME_FIELDS_MANIFEST_ID));

			//if (curActionDTO.CrateStorage == null)
			//{
			//	curActionDTO.CrateStorage = new CrateStorageDTO();
			//}
			//curActionDTO.CrateStorage.CrateDTO.AddRange(crateConfiguration);

			return curActionDTO;
		}

		private static List<PayloadMappingsDTO> GetDocusingRoleValues(Newtonsoft.Json.Linq.JToken roleToken)
		{
			List<PayloadMappingsDTO> roleValue = new List<PayloadMappingsDTO>();
			foreach (var role in roleToken)
			{
				var dict = role.ToObject<Dictionary<string, string>>();
				var singerFields = dict.Select(x => new FieldMappingDTO() { Name = x.Key, Value = x.Value });
				PayloadMappingsDTO payload = new PayloadMappingsDTO();
				payload.AddRange(singerFields);
				roleValue.Add(payload);
			}
			return roleValue;
		}
		private CrateDTO CreateStandartConfigurationControls()
		{
			var fieldSelectDocusignTemplate = new DropdownListFieldDefinitionDTO()
			{
				FieldLabel = "target_docusign_template",
				Name = "target_docusign_template",
				Required = true,
				Events = new List<FieldEvent>() {
                     new FieldEvent("onSelect", "requestConfig")
                },
				Source = new FieldSourceDTO
				{
					Label = "Available_Templates",
					ManifestType = ManifestIdEnum.StandardDesignTimeFields.GetEnumDisplayName()
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

			var crateControls = _crate.Create("Configuration_Controls", JsonConvert.SerializeObject(controls),
				ManifestIdEnum.StandardConfigurationControls.GetEnumDisplayName(), (int)ManifestIdEnum.StandardConfigurationControls);
			return crateControls;
		}
		private CrateDTO CreateStandardDesignTimeFields()
		{
			var templates = _template.GetTemplates(null);
			var fields = templates.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToList();
			var controls = new StandardDesignTimeFieldsMS()
			{
				Fields = fields,
			};

			var createDesignTimeFields = _crate.Create("Available_Templates", JsonConvert.SerializeObject(controls),
				ManifestIdEnum.StandardDesignTimeFields.GetEnumDisplayName(), (int)ManifestIdEnum.StandardDesignTimeFields);
			return createDesignTimeFields;
		}
	}
}