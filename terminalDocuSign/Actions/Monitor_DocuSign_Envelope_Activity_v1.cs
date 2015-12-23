using Data.Entities;
using TerminalBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Hub.Managers;
using Newtonsoft.Json;
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
    public class Monitor_DocuSign_Envelope_Activity_v1 : BaseTerminalAction
    {
        DocuSignManager _docuSignManager = new DocuSignManager();

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            return Crate.IsStorageEmpty(curActionDO) ? ConfigurationRequestType.Initial : ConfigurationRequestType.Followup;
            }

        protected Crate PackCrate_DocuSignTemplateNames(DocuSignAuth authDTO)
        {
            var template = new DocuSignTemplate();

            var templates = template.GetTemplates(authDTO.Email, authDTO.ApiPassword);
            var fields = templates.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToArray();
            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate(
                "Available Templates",
                fields);
            return createDesignTimeFields;
        }

        private void GetTemplateRecipientPickerValue(ActionDO curActionDO, out string selectedOption,
                                                     out string selectedValue)
        {
            GetTemplateRecipientPickerValue(Crate.GetStorage(curActionDO), out selectedOption, out selectedValue);
        }

        private void GetTemplateRecipientPickerValue(CrateStorage storage, out string selectedOption, out string selectedValue)
        {
            var controls = storage.FirstCrate<StandardConfigurationControlsCM>(x => x.Label == "Configuration_Controls");

            var group = controls.Content.Controls.OfType<RadioButtonGroup>().FirstOrDefault();
            if (group == null)
            {
                selectedOption = "template";
                selectedValue = controls.Content.Controls.OfType<DropDownList>().First().Value;
            }
            else
            {
                if (group.Radios.Any(x => x.Selected))
                {
                    //get the option which is selected from the Template/Recipient picker
                    var pickedControl = group.Radios.Single(r => r.Selected);

                    //set the output values
                    selectedOption = pickedControl.Name;
                    selectedValue = pickedControl.Controls[0].Value;
                }
                else
                {
                    selectedOption = string.Empty;
                    selectedValue = string.Empty;
                }
            }
        }

        public override Task<ActionDO> Activate(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            DocuSignAccount docuSignAccount = new DocuSignAccount();
            ConnectProfile connectProfile = docuSignAccount.GetDocuSignConnectProfiles();
            if (Int32.Parse(connectProfile.totalRecords) > 0)
            {
                return Task.FromResult<ActionDO>(curActionDO); // Will be changed when implementation is plumbed in.
            }
            else
            {
                throw new Exception("Error during activation of the Monitor DocuSign Action");
            }
        }

        public override Task<ActionDO> Deactivate(ActionDO curActionDO)
        {
            DocuSignAccount docuSignAccount = new DocuSignAccount();
            ConnectProfile connectProfile = docuSignAccount.GetDocuSignConnectProfiles();
            if (Int32.Parse(connectProfile.totalRecords) > 0)
            {
                return Task.FromResult<ActionDO>(curActionDO); // Will be changed when implementation is plumbed in.
            }
            else
            {
                throw new Exception("Error during activation of the Monitor DocuSign Action");
            }
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            //get currently selected option and its value
            string curSelectedOption, curSelectedValue;
            GetTemplateRecipientPickerValue(curActionDO, out curSelectedOption, out curSelectedValue);

            var processPayload = await GetProcessPayload(curActionDO, containerId);
            string envelopeId = string.Empty;

            //retrieve envelope ID based on the selected option and its value
            if (!string.IsNullOrEmpty(curSelectedOption))
            {
                switch (curSelectedOption)
                {
                    case "template":
                        //filter the incoming envelope by template value selected by the user
                        var curAvailableTemplates = Crate.GetStorage(curActionDO).CratesOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Available Templates").Single().Content;
                        var selectedTemplateName = curAvailableTemplates.Fields.Single(a => a.Value == curSelectedValue).Key;
                        var incommingTemplate = GetValueForKey(processPayload, "TemplateName");
                        if (selectedTemplateName == incommingTemplate)
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
            if (string.IsNullOrEmpty(envelopeId))
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "EnvelopeId");

            //Create a run-time fields
            var fields = CreateDocuSignEventFields();
            foreach (var field in fields)
            {
                field.Value = GetValueForKey(processPayload, field.Key);
            }

            //Create log message
            var logMessages = new StandardLoggingCM()
            {
                Item = new List<LogItemDTO>
                {
                    new LogItemDTO
                    {
                        Data = "Monitor DocuSign action successfully recieved an envelope ID " + envelopeId,
                        IsLogged = false
                    }
                }
            };

            using (var updater = Crate.UpdateStorage(processPayload))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Payload Data", new StandardPayloadDataCM(fields)));
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Log Messages", logMessages));
                if (curSelectedOption == "template")
                {
                    var userDefinedFieldsPayload = _docuSignManager.CreateActionPayload(curActionDO, authTokenDO, curSelectedValue);
                    updater.CrateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Data", userDefinedFieldsPayload));
            }
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

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuth>(authTokenDO.Token);

            var crateControls = PackCrate_ConfigurationControls();
            var crateDesignTimeFields = _docuSignManager.PackCrate_DocuSignTemplateNames(docuSignAuthDTO);
            var eventFields = Crate.CreateDesignTimeFieldsCrate("DocuSign Event Fields", CreateDocuSignEventFields().ToArray());

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Add(crateControls);
                updater.CrateStorage.Add(crateDesignTimeFields);
                updater.CrateStorage.Add(eventFields);

                // Remove previously added crate of "Standard Event Subscriptions" schema
                updater.CrateStorage.Remove<EventSubscriptionCM>();
                updater.CrateStorage.Add(PackCrate_EventSubscriptions(crateControls.Get<StandardConfigurationControlsCM>()));
            }
            return await Task.FromResult<ActionDO>(curActionDO);
        }

        protected override Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //just update the user selected envelope events in the follow up configuration

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                UpdateSelectedEvents(updater.CrateStorage);
                string selectedOption, selectedValue;
                GetTemplateRecipientPickerValue(curActionDO, out selectedOption, out selectedValue);
                _docuSignManager.UpdateUserDefinedFields(curActionDO, authTokenDO, updater, selectedValue);
            }



            return Task.FromResult(curActionDO);
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
                    .Where(configControl => configControl.Type.Equals(ControlTypes.CheckBox) && configControl.Selected && configControl.Name.StartsWith("Event_"))
                    .Select(checkBox => checkBox.Name.Substring("Event_".Length).Replace("_", ""));

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
                    subscriptions.Add(eventCheckBox.Name.Substring("Event_".Length).Replace("_", ""));
                }
            }

            return Crate.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                subscriptions.ToArray()
                );
        }

        private Crate PackCrate_ConfigurationControls()
        {
            var textArea = new TextArea
            {
                IsReadOnly = true,
                Label = "",
                Value = "<p>Process incoming DocuSign Envelope notifications if the following are true:</p>"
            };

            var fieldEnvelopeSent = new CheckBox()
            {
                Label = "You sent a DocuSign Envelope",
                Name = "Event_Envelope_Sent",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                }
            };

            var fieldEnvelopeReceived = new CheckBox()
            {
                Label = "Someone received an Envelope you sent",
                Name = "Event_Envelope_Received",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                }
            };

            var fieldRecipientSigned = new CheckBox()
            {
                Label = "One of your Recipients signed an Envelope",
                Name = "Event_Recipient_Signed",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                }
            };


            // remove by FR-1766
            //var fieldEventRecipientSent = new CheckBox()
            //{
            //    Label = "Recipient Sent",
            //    Name = "Event_Recipient_Sent",
            //    Events = new List<ControlEvent>()
            //    {
            //        new ControlEvent("onChange", "requestConfig")
            //    }
            //};

            return PackControlsCrate(
                textArea,
                fieldEnvelopeSent,
                fieldEnvelopeReceived,
                fieldRecipientSigned,
                PackCrate_TemplateRecipientPicker());
        }

        private ControlDefinitionDTO PackCrate_TemplateRecipientPicker()
        {
            var templateRecipientPicker = new RadioButtonGroup()
            {
                Label = "The envelope:",
                GroupName = "TemplateRecipientPicker",
                Name = "TemplateRecipientPicker",
                Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") },
                Radios = new List<RadioButtonOption>()
                {
                    new RadioButtonOption()
                    {
                        Selected = false,
                        Name = "recipient",
                        Value = "Was sent to a specific recipient",
                        Controls = new List<ControlDefinitionDTO>
                        {
                            new TextBox()
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
                        Value = "Was based on a specific template",
                        Controls = new List<ControlDefinitionDTO>
                        {
                            new DropDownList()
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

        private Crate PackCrate_TemplateNames(DocuSignAuth authDTO)
        {
            var template = new DocuSignTemplate();
            var templates = template.GetTemplates(authDTO.Email, authDTO.ApiPassword);
            var fields = templates.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToArray();
            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate(
                "Available Templates",
                fields);
            return createDesignTimeFields;
        }

        private List<FieldDTO> CreateDocuSignEventFields()
        {
            return new List<FieldDTO>(){
                new FieldDTO("RecipientEmail") {Tags = "EmailAddress" },
                new FieldDTO("DocumentName"),
                new FieldDTO("TemplateName"),
                new FieldDTO("Status"),
                new FieldDTO("CreateDate") {Tags = "Date" },
                new FieldDTO("SentDate") {Tags = "Date" },
                new FieldDTO("DeliveredDate") {Tags = "Date" },
                new FieldDTO("CompletedDate") {Tags = "Date" },
                new FieldDTO("HolderEmail") {Tags = "EmailAddress" },
                new FieldDTO("Subject")
                };
        }
    }
}
