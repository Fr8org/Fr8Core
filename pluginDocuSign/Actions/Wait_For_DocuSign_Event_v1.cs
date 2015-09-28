using Data.Entities;
using PluginBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StructureMap;
using Newtonsoft.Json;
using Core.Interfaces;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using PluginBase;
using PluginBase.BaseClasses;
using DocuSign.Integrations.Client;
using pluginDocuSign.Interfaces;
using pluginDocuSign.Infrastructure;

namespace pluginDocuSign.Actions
{
    public class Wait_For_DocuSign_Event_v1 : BasePluginAction
    {
        IAction _action = ObjectFactory.GetInstance<IAction>();
        ICrate _crate = ObjectFactory.GetInstance<ICrate>();
        IDocuSignTemplate _template = ObjectFactory.GetInstance<IDocuSignTemplate>();
        IDocuSignEnvelope _docusignEnvelope = ObjectFactory.GetInstance<IDocuSignEnvelope>();


        public ActionDTO Configure(ActionDTO curActionDTO)
        {
            //TODO: The coniguration feature for Docu Sign is not yet defined. The configuration evaluation needs to be implemented.
            return ProcessConfigurationRequest(curActionDTO, actionDo => ConfigurationEvaluator(actionDo)); // will be changed to complete the config feature for docu sign
        }

        public ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            CrateStorageDTO curCrates = curActionDTO.CrateStorage;

            if (curCrates.CrateDTO.Count == 0)
                return ConfigurationRequestType.Initial;

            //load configuration crates of manifest type Standard Control Crates
            //look for a text field name connection string with a value
            var controlsCrates = _action.GetCratesByManifestType(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME,
                curActionDTO.CrateStorage);
            var curDocuSignTemplateId = _crate.GetElementByKey(controlsCrates, key: "Selected_DocuSign_Template", keyFieldName: "name")
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

        public object Activate(ActionDTO curDataPackage)
        {
            DocuSignAccount docuSignAccount = new DocuSignAccount();
            ConnectProfile connectProfile = docuSignAccount.GetDocuSignConnectProfiles();
            if (Int32.Parse(connectProfile.totalRecords) > 0)
            {
                return "Not Yet Implemented"; // Will be changed when implementation is plumbed in.
            }
            else
            {
                return "Fail";
            }
        }

        public object Deactivate(ActionDTO curDataPackage)
        {
            DocuSignAccount docuSignAccount = new DocuSignAccount();
            ConnectProfile connectProfile = docuSignAccount.GetDocuSignConnectProfiles();
            if (Int32.Parse(connectProfile.totalRecords) > 0)
            {
                return "Not Yet Implemented"; // Will be changed when implementation is plumbed in.
            }
            else
            {
                return "Fail";
            }
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
            var eventReportCrate = curPayloadDTO.CrateStorageDTO().CrateDTO.SingleOrDefault();
            if (eventReportCrate == null)
            {
                return null;
            }

            var eventReportMS = JsonConvert.DeserializeObject<EventReportMS>(eventReportCrate.Contents);
            var crate = eventReportMS.EventPayload.SingleOrDefault();
            if (crate == null)
            {
                return null;
            }

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
				var crateControls = CreateStandardConfigurationControls();
				var crateDesignTimeFields = CreateStandardDesignTimeFields();
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

            if (configurationFields == null || !configurationFields.Controls.Any(c => c.Name == "Selected_DocuSign_Template"))
            {
                return curActionDTO;
            }

            // Remove previously added crate of the same schema
            _crate.RemoveCrateByLabel(
                curActionDTO.CrateStorage.CrateDTO,
                "DocuSignTemplateUserDefinedFields"
                );

            // Remove previously added crate of "Standard Event Subscriptions" schema
            _crate.RemoveCrateByManifestType(
                curActionDTO.CrateStorage.CrateDTO,
                "Standard Event Subscriptions"
                );

            var docusignTemplateId = configurationFields.Controls
                .SingleOrDefault(c => c.Name == "Selected_DocuSign_Template")
                .Value;
            var userDefinedFields = _docusignEnvelope
                .GetEnvelopeDataByTemplate(docusignTemplateId);

            var crateConfiguration = new List<CrateDTO>();

            var fieldCollection = userDefinedFields
                .Select(f => new FieldDTO()
            {
                Key = f.Name,
                Value = f.Value
                })
                .ToArray();

            crateConfiguration.Add(
                _crate.CreateDesignTimeFieldsCrate(
                "DocuSignTemplateUserDefinedFields",
                    fieldCollection
                    )
                );

            crateConfiguration.Add(
                CreateEventSubscriptionCrate(configurationFields)
                );

            if (curActionDTO.CrateStorage == null)
            {
                curActionDTO.CrateStorage = new CrateStorageDTO();
            }

            curActionDTO.CrateStorage.CrateDTO.AddRange(crateConfiguration);

            return curActionDTO;
        }

        private CrateDTO CreateEventSubscriptionCrate(
            StandardConfigurationControlsMS configurationFields)
        {
            var subscriptions = new List<string>();

            var eventCheckBoxes = configurationFields.Controls
                .Where(x => x.Type == "checkboxField" && x.Name.StartsWith("Event_"));

            foreach (var eventCheckBox in eventCheckBoxes)
            {
                if (eventCheckBox.Selected)
                {
                    subscriptions.Add(eventCheckBox.Label);
                }
            }

            return _crate.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                subscriptions.ToArray()
                );
        }

        private CrateDTO CreateStandardConfigurationControls()
        {
            var fieldSelectDocusignTemplate = new DropdownListFieldDefinitionDTO()
            {
	            Label = "Select DocuSign Template",
	            Name = "Selected_DocuSign_Template",
	            Required = true,
	            Events = new List<FieldEvent>()
                {
                    new FieldEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                }
            };

            var fieldEnvelopeSent = new CheckBoxFieldDefinitionDTO()
            {
                Label = "Envelope Sent",
                Name = "Event_Envelope_Sent"
            };

				var fieldEnvelopeReceived = new CheckBoxFieldDefinitionDTO()
            {
                Label = "Envelope Received",
                Name = "Event_Envelope_Received"
            };

				var fieldRecipientSigned = new CheckBoxFieldDefinitionDTO()
            {
                Label = "Recipient Signed",
                Name = "Event_Recipient_Signed"
            };

				var fieldEventRecipientSent = new CheckBoxFieldDefinitionDTO()
            {
                Label = "Recipient Sent",
                Name = "Event_Recipient_Sent"
            };

            return PackControlsCrate(
                fieldSelectDocusignTemplate,
                fieldEnvelopeSent,
                fieldEnvelopeReceived,
                fieldRecipientSigned,
                fieldEventRecipientSent);
        }

        private CrateDTO CreateStandardDesignTimeFields()
        {
            var templates = _template.GetTemplates(null);
            var fields = templates.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToArray();
            var createDesignTimeFields = _crate.CreateDesignTimeFieldsCrate(
                "Available Templates",
                fields);
            return createDesignTimeFields;
        }
    }
}