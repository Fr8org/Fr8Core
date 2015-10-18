using AutoMapper;
using Data.Entities;
using PluginBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using PluginBase;
using DocuSign.Integrations.Client;
using pluginDocuSign.DataTransferObjects;
using pluginDocuSign.Infrastructure;
using pluginDocuSign.Services;
using PluginUtilities.BaseClasses;

namespace pluginDocuSign.Actions
{
    public class Monitor_DocuSign_v1 : BasePluginAction
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
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        private string GetSelectedTemplateId(ActionDTO curActionDTO)
        {
            var controlsCrates = Crate.GetCratesByManifestType(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME,
                curActionDTO.CrateStorage);
            var curDocuSignTemplateId = Crate.GetElementByKey(controlsCrates, key: "Selected_DocuSign_Template",
                keyFieldName: "name")
                .Select(e => (string) e["value"])
                .FirstOrDefault(s => !string.IsNullOrEmpty(s));
            return curDocuSignTemplateId;
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

        public async Task<PayloadDTO> Run(ActionDTO actionDto)
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

            var cratePayload = Crate.Create(
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

            var eventReportMS = JsonConvert.DeserializeObject<EventReportCM>(eventReportCrate.Contents);
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

            var crateControls = PackCrate_ConfigurationControls();
			var crateDesignTimeFields = PackCrate_TemplateNames(docuSignAuthDTO);
            var eventFields = PackCrate_DocuSignEventFields();

            curActionDTO.CrateStorage.CrateDTO.Add(crateControls);
			curActionDTO.CrateStorage.CrateDTO.Add(crateDesignTimeFields);
            curActionDTO.CrateStorage.CrateDTO.Add(eventFields);

            var configurationFields = Crate.GetConfigurationControls(Mapper.Map<ActionDO>(curActionDTO));

            // Remove previously added crate of "Standard Event Subscriptions" schema

            Crate.ReplaceCratesByManifestType(curActionDTO.CrateStorage.CrateDTO,
                CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_NAME,
                new List<CrateDTO> {PackCrate_EventSubscriptions(configurationFields)});

            return await Task.FromResult<ActionDTO>(curActionDTO);
        }

        protected override Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            string curSelectedTemplateId = GetSelectedTemplateId(curActionDTO);

            if (!string.IsNullOrEmpty(curSelectedTemplateId))
            {
                //get the existing DocuSign event fields
                var curEventFieldsCrate = Crate.GetCratesByLabel("DocuSign Event Fields", curActionDTO.CrateStorage).Single();
                var curEventFields = Crate.GetStandardDesignTimeFields(curEventFieldsCrate);

                //set the selected template ID
                curEventFields.Fields.ForEach(field =>
                {
                    if (field.Key.Equals("TemplateId"))
                    {
                        field.Value = curSelectedTemplateId;
                    }
                });


                //update the DocuSign Event Fields with new value
                Crate.ReplaceCratesByLabel(curActionDTO.CrateStorage.CrateDTO, "DocuSign Event Fields",
                    new List<CrateDTO>
                    {
                        Crate.CreateDesignTimeFieldsCrate("DocuSign Event Fields", curEventFields.Fields.ToArray())
                    });
            }

            return Task.FromResult(curActionDTO);
        }

        private CrateDTO PackCrate_EventSubscriptions(
            StandardConfigurationControlsCM configurationFields)
        {
            var subscriptions = new List<string>();

            var eventCheckBoxes = configurationFields.Controls
                .Where(x => x.Type == "CheckBox" && x.Name.StartsWith("Event_"));

            foreach (var eventCheckBox in eventCheckBoxes)
            {
                if (eventCheckBox.Selected)
                {
                    subscriptions.Add(eventCheckBox.Label);
                }
            }

            return Crate.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                subscriptions.ToArray()
                );
        }

        private CrateDTO PackCrate_ConfigurationControls()
        {
            var fieldSelectDocusignTemplate = new DropDownListControlDefinitionDTO()
            {
	            Label = "Select DocuSign Template",
	            Name = "Selected_DocuSign_Template",
	            Required = true,
	            Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                }
            };

            var fieldEnvelopeSent = new CheckBoxControlDefinitionDTO()
            {
                Label = "Envelope Sent",
                Name = "Event_Envelope_Sent",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                }
            };

				var fieldEnvelopeReceived = new CheckBoxControlDefinitionDTO()
            {
                Label = "Envelope Received",
                Name = "Event_Envelope_Received",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                }
            };

				var fieldRecipientSigned = new CheckBoxControlDefinitionDTO()
                {
                Label = "Recipient Signed",
                Name = "Event_Recipient_Signed",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                }
            };

				var fieldEventRecipientSent = new CheckBoxControlDefinitionDTO()
                {
                Label = "Recipient Sent",
                Name = "Event_Recipient_Sent",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                }
            };

            return PackControlsCrate(
                fieldSelectDocusignTemplate,
                fieldEnvelopeSent,
                fieldEnvelopeReceived,
                fieldRecipientSigned,
                fieldEventRecipientSent);
        }

        private CrateDTO PackCrate_TemplateNames(DocuSignAuthDTO authDTO)
        {
            var template = new DocuSignTemplate();

            var templates = template.GetTemplates(authDTO.Email, authDTO.ApiPassword);
            var fields = templates.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToArray();
            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate(
                "Available Templates",
                fields);
            return createDesignTimeFields;
        }

        private CrateDTO PackCrate_DocuSignEventFields()
        {
            return Crate.CreateDesignTimeFieldsCrate("DocuSign Event Fields",
                new FieldDTO {Key = "EnvelopeId", Value = string.Empty},
                new FieldDTO {Key = "TemplateId", Value = string.Empty});
        }
    }
}