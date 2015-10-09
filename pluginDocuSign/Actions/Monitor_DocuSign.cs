using Data.Entities;
using PluginBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using pluginDocuSign.DataTransferObjects;
using pluginDocuSign.Interfaces;
using pluginDocuSign.Infrastructure;
using pluginDocuSign.Services;

namespace pluginDocuSign.Actions
{
    public class Monitor_DocuSign : BasePluginAction
    {
        // TODO: remove this as of DO-1064.
        // IDocuSignTemplate _template = ObjectFactory.GetInstance<IDocuSignTemplate>();
        // IDocuSignEnvelope _docusignEnvelope = ObjectFactory.GetInstance<IDocuSignEnvelope>();

        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            if (IsEmptyAuthToken(curActionDTO))
            {
                AppendDockyardAuthenticationCrate(curActionDTO, AuthenticationMode.InternalMode);
                return curActionDTO;
            }

            RemoveAuthenticationCrate(curActionDTO);

            return await ProcessConfigurationRequest(curActionDTO, actionDo => ConfigurationEvaluator(actionDo));
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

        public async Task<PayloadDTO> Execute(ActionDTO actionDto)
        {
            if (IsEmptyAuthToken(actionDto))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            var processPayload = await GetProcessPayload(actionDto.ProcessId);

            // Extract envelope id from the payload Crate
            string envelopeId = GetEnvelopeId(processPayload);

            // Make sure that it exists
            if (String.IsNullOrEmpty(envelopeId))
                throw new PluginCodedException(PluginErrorCode.PAYLOAD_DATA_MISSING, "EnvelopeId");

            //Create a field
            var fields = new List<FieldDTO>()
            {
                new FieldDTO()
                {
                    Key = "EnvelopeId",
                    Value = envelopeId
                }
            };

            var cratePayload = _crate.Create(
                "DocuSign Envelope Payload Data",
                JsonConvert.SerializeObject(fields),
                CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME,
                CrateManifests.STANDARD_PAYLOAD_MANIFEST_ID
                );

            processPayload.UpdateCrateStorageDTO(new List<CrateDTO>() { cratePayload });

            return processPayload;
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

        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            var docuSignAuthDTO = JsonConvert
                .DeserializeObject<DocuSignAuthDTO>(curActionDTO.AuthToken.Token);

            if (curActionDTO.CrateStorage == null)
            {
                curActionDTO.CrateStorage = new CrateStorageDTO();
            }

            var crateControls = CreateConfigurationCrate();
            var crateDesignTimeFields = CreateDesignFieldsCrate_TemplateNames(docuSignAuthDTO);
            curActionDTO.CrateStorage.CrateDTO.Add(crateControls);
            curActionDTO.CrateStorage.CrateDTO.Add(crateDesignTimeFields);

            var curCrates = curActionDTO.CrateStorage.CrateDTO;

            //extract Docusign template Id
            var configurationFieldsCrate = curCrates.SingleOrDefault(c => c.Label == "Configuration_Controls");
            if (configurationFieldsCrate == null || String.IsNullOrEmpty(configurationFieldsCrate.Contents))
            {
                return await Task.FromResult<ActionDTO>(curActionDTO);
            }

            var configurationFields = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(configurationFieldsCrate.Contents);
            if (configurationFields == null || !configurationFields.Controls.Any(c => c.Name == "Selected_DocuSign_Template"))
            {
                return await Task.FromResult<ActionDTO>(curActionDTO);
            }

            // Remove previously added crate of "Standard Event Subscriptions" schema
            _crate.RemoveCrateByManifestType(
                curActionDTO.CrateStorage.CrateDTO,
                CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_NAME
                );

            var crateConfiguration = new List<CrateDTO>();

            crateConfiguration.Add(
                CreateEventSubscriptionCrate(configurationFields)
                );

            curActionDTO.CrateStorage.CrateDTO.AddRange(crateConfiguration);

            return await Task.FromResult<ActionDTO>(curActionDTO);
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

        private CrateDTO CreateConfigurationCrate()
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

        private CrateDTO CreateDesignFieldsCrate_TemplateNames(DocuSignAuthDTO authDTO)
        {
            var template = new DocuSignTemplate();

            var templates = template.GetTemplates(authDTO.Email, authDTO.ApiPassword);
            var fields = templates.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToArray();
            var createDesignTimeFields = _crate.CreateDesignTimeFieldsCrate(
                "Available Templates",
                fields);
            return createDesignTimeFields;
        }
    }
}