
﻿using AutoMapper;
using Data.Entities;
using TerminalBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Hub.Managers;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using TerminalBase;
using DocuSign.Integrations.Client;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Services;
using TerminalBase.BaseClasses;

namespace terminalDocuSign.Actions
{
    public class Monitor_DocuSign_v1 : BaseTerminalAction
    {
        DocuSignManager _docuSignManager = new DocuSignManager();

        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            if (NeedsAuthentication(curActionDTO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActionDTO, x => ConfigurationEvaluator(x));
        }

        public ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            if (Crate.IsEmptyStorage(curActionDTO.CrateStorage))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected Crate PackCrate_DocuSignTemplateNames(DocuSignAuthDTO authDTO)
        {
            var template = new DocuSignTemplate();

            var templates = template.GetTemplates(authDTO.Email, authDTO.ApiPassword);
            var fields = templates.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToArray();
            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate(
                "Available Templates",
                fields);
            return createDesignTimeFields;
        }

        private void GetTemplateRecipientPickerValue(ActionDTO curActionDTO, out string selectedOption, out string selectedValue)
        {
            GetTemplateRecipientPickerValue(Crate.GetStorage(curActionDTO), out selectedOption, out selectedValue);
        }

        private void GetTemplateRecipientPickerValue(CrateStorage storage, out string selectedOption, out string selectedValue)
        {
            var controls = storage.FirstCrate<StandardConfigurationControlsCM>(x => x.Label == "Configuration_Controls");

            var group = controls.Content.Controls.OfType<RadioButtonGroupControlDefinitionDTO>().FirstOrDefault();
            if (group == null)
            {
                selectedOption = "template";
                selectedValue = controls.Content.Controls.OfType<DropDownListControlDefinitionDTO>().First().Value;
            }
            else
            {
                //get the option which is selected from the Template/Recipient picker
                var pickedControl = group.Radios.Single(r => r.Selected);

                //set the output values
                selectedOption = pickedControl.Name;
                selectedValue = pickedControl.Controls[0].Value;
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

        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            if (NeedsAuthentication(curActionDTO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            //get currently selected option and its value
            string curSelectedOption, curSelectedValue;
            GetTemplateRecipientPickerValue(curActionDTO, out curSelectedOption, out curSelectedValue);

            var processPayload = await GetProcessPayload(curActionDTO.ProcessId);

            string envelopeId = string.Empty;

            //retrieve envelope ID based on the selected option and its value
            if (!string.IsNullOrEmpty(curSelectedOption))
            {
                switch (curSelectedOption)
                {
                    case "template":
                        //filter the incoming envelope by template value selected by the user
                        var curAvailableTemplates = Crate.GetStorage(curActionDTO).CratesOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Available Templates").Single().Content;

                        //if the incoming enveloped is prepared using selected template, get the envelope ID
                        if (curAvailableTemplates.Fields.Single(field => field.Value.Equals(curSelectedValue)).Value.Equals(curSelectedValue))
                        {
                            envelopeId = GetValueForKey(processPayload, "EnvelopeId");
                        }

                        break;
                    case "recipient":
                        //filter incoming envelope by recipient email address specified by the user
                        var curRecipientEmail = GetValueForKey(processPayload, "RecipientEmail");

                        //if the incoming envelope's recipient is user specified one, get the envelope ID
                        if (curRecipientEmail.Equals(curSelectedValue))
                        {
                            envelopeId = GetValueForKey(processPayload, "EnvelopeId");
                        }
                        break;
                }
            }



            // Make sure that it exists
            if (String.IsNullOrEmpty(envelopeId))
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "EnvelopeId");

            //Create a field
            var fields = new List<FieldDTO>()
            {
                new FieldDTO()
                {
                    Key = "EnvelopeId",
                    Value = envelopeId
                }
            };

            using (var updater = Crate.UpdateStorage(processPayload))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Payload Data", new StandardPayloadDataCM(fields)));
            }

            return processPayload;
        }

        private string GetValueForKey(PayloadDTO curPayloadDTO, string curKey)
        {
            var eventReportMS = Crate.GetStorage(curPayloadDTO).CrateContentsOfType<EventReportCM>().FirstOrDefault();

            if (eventReportMS == null)
            {
                return null;
            }

            var crate = eventReportMS.EventPayload.CratesOfType<StandardPayloadDataCM>().First();
            
            if (crate == null)
            {
                return null;
            }

            var fields = crate.Content.AllValues().ToArray();
            if (fields == null || fields.Length == 0) return null;

            var envelopeIdField = fields.SingleOrDefault(f => f.Key == curKey);
            if (envelopeIdField == null) return null;

            return envelopeIdField.Value;
        }

        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            var docuSignAuthDTO = JsonConvert
                .DeserializeObject<DocuSignAuthDTO>(curActionDTO.AuthToken.Token);

         
            var crateControls = PackCrate_ConfigurationControls();
            var crateDesignTimeFields = _docuSignManager.PackCrate_DocuSignTemplateNames(docuSignAuthDTO);
            var eventFields = PackCrate_DocuSignEventFields();


            using (var updater = Crate.UpdateStorage(curActionDTO))
            {
                updater.CrateStorage.Add(crateControls);
                updater.CrateStorage.Add(crateDesignTimeFields);
                updater.CrateStorage.Add(eventFields);

                // Remove previously added crate of "Standard Event Subscriptions" schema
                updater.CrateStorage.Remove<EventSubscriptionCM>();
                updater.CrateStorage.Add(PackCrate_EventSubscriptions(crateControls.Get<StandardConfigurationControlsCM>()));
            }
            return await Task.FromResult<ActionDTO>(curActionDTO);
        }

        protected override Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            //just update the user selected envelope events in the follow up configuration

            using (var updater = Crate.UpdateStorage(curActionDTO))
            {
                UpdateSelectedTemplateId(updater.CrateStorage);
                UpdateSelectedEvents(updater.CrateStorage);
            }

            return Task.FromResult(curActionDTO);
        }

        private void UpdateSelectedTemplateId(CrateStorage storage)
        {
            string selectedOption, selectedValue;

            GetTemplateRecipientPickerValue(storage, out selectedOption, out selectedValue);

            if (selectedOption == "template")
            {
                var envelopeIdField = storage
                    .CrateContentsOfType<StandardDesignTimeFieldsCM>()
                    .SelectMany(x => x.Fields)
                    .FirstOrDefault(x => x.Key == "EnvelopeId");

                if (envelopeIdField != null)
                {
                    envelopeIdField.Value = selectedValue;
                }
            }
        }

        /// <summary>
        /// Updates event subscriptions list by user checked check boxes.
        /// </summary>
        /// <remarks>The configuration controls include check boxes used to get the selected DocuSign event subscriptions</remarks>
        private void UpdateSelectedEvents(CrateStorage storage)
        {
            //get the config controls manifest

            var curConfigControlsCrate = storage.CrateContentsOfType<StandardConfigurationControlsCM>().First();

            //get selected check boxes (i.e. user wanted to subscribe these DocuSign events to monitor for)
            var curSelectedDocuSignEvents =
                curConfigControlsCrate.Controls
                    .Where(configControl => configControl.Type.Equals(ControlTypes.CheckBox) && configControl.Selected)
                    .Select(checkBox => checkBox.Label.Replace(" ", ""));

            //create standard event subscription crate with user selected DocuSign events
            var curEventSubscriptionCrate = Crate.CreateStandardEventSubscriptionsCrate("Standard Event Subscriptions",
                curSelectedDocuSignEvents.ToArray());

            storage.Remove<EventSubscriptionCM>();
            storage.Add(curEventSubscriptionCrate);
        }

        private Crate PackCrate_EventSubscriptions(StandardConfigurationControlsCM configurationFields)
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

        private Crate PackCrate_ConfigurationControls()
        {
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
                PackCrate_TemplateRecipientPicker(),
                fieldEnvelopeSent,
                fieldEnvelopeReceived,
                fieldRecipientSigned,
                fieldEventRecipientSent);
        }

        private ControlDefinitionDTO PackCrate_TemplateRecipientPicker()
        {
            var templateRecipientPicker = new RadioButtonGroupControlDefinitionDTO()
            {
                Label = "Monitor for Envelopes that:",
                GroupName = "TemplateRecipientPicker",
                Name = "TemplateRecipientPicker",
                Events = new List<ControlEvent> {new ControlEvent("onChange", "requestConfig")},
                Radios = new List<RadioButtonOption>()
                {
                    new RadioButtonOption()
                    {
                        Selected = true,
                        Name = "recipient",
                        Value = "Are sent to the recipient:",
                        Controls = new List<ControlDefinitionDTO>
                        {
                            new TextBoxControlDefinitionDTO()
                            {
                                Label = "",
                                Name = "RecipientValue",
                                Events = new List<ControlEvent> {new ControlEvent("onChange", "requestConfig")}
                            }
                        }
                    },

                    new RadioButtonOption()
                    {
                        Selected = false,
                        Name = "template",
                        Value = "Use the template:",
                        Controls = new List<ControlDefinitionDTO>
                        {
                            new DropDownListControlDefinitionDTO()
                            {
                                Label = "",
                                Name = "UpstreamCrate",
                                Source = new FieldSourceDTO
                                {
                                    Label = "Available Templates",
                                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                                },
                                Events = new List<ControlEvent> {new ControlEvent("onChange", "requestConfig")}
                            }
                        }
                    }
                }
            };

            return templateRecipientPicker;
        }

        private Crate PackCrate_TemplateNames(DocuSignAuthDTO authDTO)
        {
            var template = new DocuSignTemplate();

            var templates = template.GetTemplates(authDTO.Email, authDTO.ApiPassword);
            var fields = templates.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToArray();
            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate(
                "Available Templates",
                fields);
            return createDesignTimeFields;
        }

        private Crate PackCrate_DocuSignEventFields()
        {
            return Crate.CreateDesignTimeFieldsCrate("DocuSign Event Fields",
                new FieldDTO {Key = "EnvelopeId", Value = string.Empty});
        }
    }
}