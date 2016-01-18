using System.Web.UI;
using Data.Constants;
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
using Utilities.Configuration.Azure;
using Data.States;

namespace terminalDocuSign.Actions
{
    public class Monitor_DocuSign_Envelope_Activity_v1 : BaseDocuSignAction
    {
        readonly DocuSignManager _docuSignManager = new DocuSignManager();

        private const string DocuSignConnectName = "fr8DocuSignConnectConfiguration";

        private const string DocuSignOnEnvelopeSentEvent = "Sent";
        private const string DocuSignOnEnvelopeReceivedEvent = "Delivered";
        private const string DocuSignOnEnvelopeSignedEvent = "Completed";

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            return Crate.IsStorageEmpty(curActionDO)
                ? ConfigurationRequestType.Initial
                : ConfigurationRequestType.Followup;
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

        private void GetUserSelectedEnvelopeEvents(ActionDO curActionDO, out bool youSent, out bool someoneReceived, out bool recipientSigned)
        {
            var configControls = GetConfigurationControls(curActionDO);
            var eventCheckBoxes = configControls.Controls.Where(c => c.Type == ControlTypes.CheckBox).ToList();
            youSent = eventCheckBoxes.Any(c => c.Name == "Event_Envelope_Sent" && c.Selected);
            someoneReceived = eventCheckBoxes.Any(c => c.Name == "Event_Envelope_Received" && c.Selected);
            recipientSigned = eventCheckBoxes.Any(c => c.Name == "Event_Recipient_Signed" && c.Selected);
        }

        private string GetDocusignPublishUrl()
        {
            var endPoint = CloudConfigurationManager.GetSetting("TerminalEndpoint");
            return "http://" + endPoint + "/terminals/terminalDocuSign/events";
        }

        public override Task<ActionDO> Activate(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //create DocuSign account, publish URL and other user selected options
            bool youSent, someoneReceived, recipientSigned;
            GetUserSelectedEnvelopeEvents(curActionDO, out youSent, out someoneReceived, out recipientSigned);

            //create or update the DocuSign connect profile configuration
            CreateOrUpdateDocuSignConnectConfiguration(youSent, someoneReceived, recipientSigned);

            return Task.FromResult<ActionDO>(curActionDO);
        }

        /// <summary>
        /// Tries to get existing Docusign connect configuration named "DocuSignConnectName" for current user
        /// </summary>
        private Configuration GetDocuSignConnectConfiguration(DocuSignAccount account)
        {
            //get all connect profiles from DocuSign for the given account
            var connectProfile = account.GetDocuSignConnectProfiles();

            //if DocuSignConnectName is already present, return the config
            if (connectProfile.configurations.Any(config => config.name == DocuSignConnectName))
            {
                return connectProfile.configurations.First(config => config.name == DocuSignConnectName);
            }

            //if nothing found, return NULL
            return null;
        }

        /// <summary>
        /// Creates or Updates a Docusign connect configuration named "DocuSignConnectName" for current user
        /// </summary>
        private void CreateOrUpdateDocuSignConnectConfiguration(bool youSent,
                                                                bool someoneReceived, bool recipientSigned)
        {
            //prepare envelope events based on the input parameters
            var envelopeEvents = "";
            if (youSent)
            {
                envelopeEvents = DocuSignOnEnvelopeSentEvent;
            }
            if (someoneReceived)
            {
                if (envelopeEvents.Length > 0)
                {
                    envelopeEvents += ",";
                }
                envelopeEvents += DocuSignOnEnvelopeReceivedEvent;
            }
            if (recipientSigned)
            {
                if (envelopeEvents.Length > 0)
                {
                    envelopeEvents += ",";
                }
                envelopeEvents += DocuSignOnEnvelopeSignedEvent;
            }

            //get existing connect configuration
            DocuSignAccount.CreateOrUpdateDefaultDocuSignConnectConfiguration(envelopeEvents);
        }

        public override Task<ActionDO> Deactivate(ActionDO curActionDO)
        {
            //get existing DocuSign connect profile
            var docuSignAccount = new DocuSignAccount();
            var existingConfig = GetDocuSignConnectConfiguration(docuSignAccount);

            //if there is a config existing, delete the connect configuration
            if (existingConfig != null)
            {
                docuSignAccount.DeleteDocuSignConnectProfile(existingConfig.connectId);
            }

            return Task.FromResult<ActionDO>(curActionDO);
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActionDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            //get currently selected option and its value
            string curSelectedOption, curSelectedValue;
            GetTemplateRecipientPickerValue(curActionDO, out curSelectedOption, out curSelectedValue);


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
                        var incommingTemplate = GetValueForKey(payloadCrates, "TemplateName");
                        if (selectedTemplateName == incommingTemplate)
                        {
                            envelopeId = GetValueForKey(payloadCrates, "EnvelopeId");
                        }
                        else
                        {
                            //this event isn't about us let's stop execution
                            return TerminateHubExecution(payloadCrates);
                        }

                        break;
                    case "recipient":
                        //filter incoming envelope by recipient email address specified by the user
                        var curRecipientEmail = GetValueForKey(payloadCrates, "RecipientEmail");

                        //if the incoming envelope's recipient is user specified one, get the envelope ID
                        if (curRecipientEmail.Equals(curSelectedValue))
                        {
                            envelopeId = GetValueForKey(payloadCrates, "EnvelopeId");
                        }
                        else
                        {
                            //this event isn't about us let's stop execution
                            return TerminateHubExecution(payloadCrates);
                        }
                        break;
                }
            }

            // Make sure that it exists
            if (string.IsNullOrEmpty(envelopeId))
            {
                return Error(payloadCrates, "EnvelopeId", ActionErrorCode.PAYLOAD_DATA_MISSING);
            }


            //Create run-time fields
            var fields = CreateDocuSignEventFields();
            foreach (var field in fields)
            {
                field.Value = GetValueForKey(payloadCrates, field.Key);
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

            using (var updater = Crate.UpdateStorage(payloadCrates))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Payload Data", new StandardPayloadDataCM(fields)));
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Log Messages", logMessages));
                if (curSelectedOption == "template")
                {
                    var userDefinedFieldsPayload = _docuSignManager.CreateActionPayload(curActionDO, authTokenDO, curSelectedValue);
                    updater.CrateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Data", userDefinedFieldsPayload));
                }
            }

            return Success(payloadCrates);
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

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

        protected override Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO,
                                                                        AuthorizationTokenDO authTokenDO)
        {
            //just update the user selected envelope events in the follow up configuration

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                UpdateSelectedEvents(updater.CrateStorage);
                string selectedOption, selectedValue;
                GetTemplateRecipientPickerValue(curActionDO, out selectedOption, out selectedValue);
                _docuSignManager.UpdateUserDefinedFields(curActionDO, authTokenDO, updater, selectedValue);
            }

            return Task.FromResult<ActionDO>(curActionDO);
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
                                Events = new List<ControlEvent> {new ControlEvent("onChange", "requestConfig")},
                                Help = new HelpControlDTO("Monitor_DocuSign_Envelope_DocuSignTemplateHelp", "Minicon")
                            }
                        }
                    }
                }
            };

            return templateRecipientPicker;
        }


        
    }
}
