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

namespace pluginDocuSign.Actions
{
    public class Wait_For_DocuSign_Event_v1 : BasePluginAction
    {
        IAction _action = ObjectFactory.GetInstance<IAction>();
        ICrate _crate = ObjectFactory.GetInstance<ICrate>();
        IDocuSignTemplate _template = ObjectFactory.GetInstance<IDocuSignTemplate>();
        IEnvelope _docusignEnvelope = ObjectFactory.GetInstance<IEnvelope>();


        public object Configure(ActionDTO curActionDTO)
        {
            //TODO: The coniguration feature for Docu Sign is not yet defined. The configuration evaluation needs to be implemented.
            return ProcessConfigurationRequest(curActionDTO,
                actionDo => ConfigurationRequestType.Initial); // will be changed to complete the config feature for docu sign
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

        protected override CrateStorageDTO InitialConfigurationResponse(ActionDTO curActionDTO)
        {

            if (curActionDTO.CrateStorage == null)
            {
                curActionDTO.CrateStorage = new CrateStorageDTO();
            }
				var crateControls = CreateStandartConfigurationControls();
				var crateDesignTimeFields = CreateStandardDesignTimeFields();
				curActionDTO.CrateStorage.CrateDTO.Add(crateControls);
				curActionDTO.CrateStorage.CrateDTO.Add(crateDesignTimeFields);

            return curActionDTO.CrateStorage;
        }
        protected override CrateStorageDTO FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            var curCrates = curActionDTO.CrateStorage.CrateDTO;

            if (curCrates == null || curCrates.Count == 0)
            {
                return curActionDTO.CrateStorage;
            }

            // Extract DocuSign Template Id
            var configurationFieldsCrate = curCrates.SingleOrDefault(c => c.Label == "Configuration_Controls");

            if (configurationFieldsCrate == null || String.IsNullOrEmpty(configurationFieldsCrate.Contents))
            {
                return curActionDTO.CrateStorage;
            }

            var configurationFields = JsonConvert.DeserializeObject<List<FieldDefinitionDTO>>(configurationFieldsCrate.Contents);

            if (configurationFields == null || !configurationFields.Any(c => c.Name == "Selected_DocuSign_Template"))
            {
                return curActionDTO.CrateStorage;
            }

            var docusignTemplateId = configurationFields.SingleOrDefault(c => c.Name == "Selected_DocuSign_Template").Value;
            var userDefinedFields = _docusignEnvelope.GetEnvelopeDataByTemplate(docusignTemplateId);
            var crateConfiguration = new List<CrateDTO>();
            var fieldCollection = userDefinedFields.Select(f => new FieldDefinitionDTO()
            {
                FieldLabel = f.Name,
                Type = f.Type,
                Name = f.Name,
                Value = f.Value
            });

            crateConfiguration.Add(_crate.Create(
                "DocuSignTemplateUserDefinedFields",
                JsonConvert.SerializeObject(fieldCollection),
                DESIGNTIME_FIELDS_MANIFEST_NAME,
                DESIGNTIME_FIELDS_MANIFEST_ID));

            //crateConfiguration.Add(_crate.Create(
            //    "DocuSignEnvelopeStandardFields", 
            //    JsonConvert.SerializeObject(fieldCollection), 
            //    "DocuSignEnvelopeStandardFields"));

            if (curActionDTO.CrateStorage == null)
            {
                curActionDTO.CrateStorage = new CrateStorageDTO();
            }
            curActionDTO.CrateStorage.CrateDTO.AddRange(crateConfiguration);
            return curActionDTO.CrateStorage;
        }
		  private CrateDTO CreateStandartConfigurationControls()
		  {
			  var fieldSelectDocusignTemplate = new FieldDefinitionDTO()
			  {
				  FieldLabel = "Select DocuSign Template",
				  Type = "dropdownlistField",
				  Name = "Selected_DocuSign_Template",
				  Required = true,
				  Events = new List<FieldEvent>() {
                     new FieldEvent("onSelect", "requestConfiguration")
                }
			  };

			  var fieldEnvelopeSent = new FieldDefinitionDTO()
			  {
				  FieldLabel = "Envelope Sent",
				  Type = "checkboxField",
				  Name = "Event_Envelope_Sent"
			  };

			  var fieldEnvelopeReceived = new FieldDefinitionDTO()
			  {
				  FieldLabel = "Envelope Received",
				  Type = "checkboxField",
				  Name = "Event_Envelope_Received"
			  };

			  var fieldRecipientSigned = new FieldDefinitionDTO()
			  {
				  FieldLabel = "Recipient Signed",
				  Type = "checkboxField",
				  Name = "Event_Recipient_Signed"
			  };

			  var fieldEventRecipientSent = new FieldDefinitionDTO()
			  {
				  FieldLabel = "Recipient Sent",
				  Type = "checkboxField",
				  Name = "Event_Recipient_Sent"
			  };

			  var fields = new List<FieldDefinitionDTO>()
            {
                fieldSelectDocusignTemplate,
                fieldEnvelopeSent,
                fieldEnvelopeReceived,
                fieldRecipientSigned,
                fieldEventRecipientSent
            };

			  var crateControls = _crate.Create("Configuration_Controls", JsonConvert.SerializeObject(fields), "Standard Configuration Controls");
			  return crateControls;
		  }
		  private CrateDTO CreateStandardDesignTimeFields()
		  {
			  var templates = _template.GetTemplates(null);
			  var fields = templates.Select(x => new FieldDTO() { Key = x.Id, Value = x.Name }).ToList();
			  var createDesignTimeFields = _crate.Create("Available Templates", JsonConvert.SerializeObject(fields), "Standard Design-Time Fields", 3);
			  return createDesignTimeFields;
		  }
    }
}