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
using DocuSign.Integrations.Client;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Services;
using Data.States;

namespace terminalDocuSign.Actions
{
    public class Monitor_DocuSign_Envelope_Activity_v1 : BaseDocuSignActivity
    {
        readonly DocuSignManager _docuSignManager = new DocuSignManager();

        private const string DocuSignConnectName = "fr8DocuSignConnectConfiguration";

        private const string DocuSignOnEnvelopeSentEvent = "Sent";
        private const string DocuSignOnEnvelopeReceivedEvent = "Delivered";
        private const string DocuSignOnEnvelopeSignedEvent = "Completed";

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);
            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            return CrateManager.IsStorageEmpty(curActivityDO)
                ? ConfigurationRequestType.Initial
                : ConfigurationRequestType.Followup;
        }

        private void GetTemplateRecipientPickerValue(ActivityDO curActivityDO, out string selectedOption, out string selectedValue, out string selectedTemplate)
        {
            var controls = CrateManager.GetStorage(curActivityDO).FirstCrate<StandardConfigurationControlsCM>(x => x.Label == "Configuration_Controls");
            selectedTemplate = string.Empty;
            var @group = controls.Content.Controls.OfType<RadioButtonGroup>().FirstOrDefault();
            if (@group == null)
            {
                selectedOption = "template";
                selectedValue = controls.Content.Controls.OfType<DropDownList>().First().Value;
            }
            else
            {
                if (@group.Radios.Any(x => x.Selected))
                {
                    //get the option which is selected from the Template/Recipient picker
                    var pickedControl = @group.Radios.Single(r => r.Selected);
                    if (pickedControl.Controls[0].Type == ControlTypes.DropDownList)
                    {
                        var templateControl = pickedControl.Controls[0] as DropDownList;
                        var selectedListItem = templateControl
                            .ListItems
                            .FirstOrDefault(x => x.Value == templateControl.Value);

                        if (selectedListItem != null)
                        {
                            selectedTemplate = selectedListItem.Key;
                        }
                    }
                    //set the output values
                    selectedOption = pickedControl.Name;
                    selectedValue = pickedControl.Controls[0].Value;
                }
                else
                {
                    selectedOption = string.Empty;
                    selectedValue = string.Empty;
                    selectedTemplate = string.Empty;
                }
            }
            //selectedTemplate = selectedOption == "template" ? selectedValue : string.Empty;
        }

        private void GetUserSelectedEnvelopeEvents(ActivityDO curActivityDO, out bool youSent, out bool someoneReceived, out bool recipientSigned)
        {
            var configControls = GetConfigurationControls(curActivityDO);
            var eventCheckBoxes = configControls.Controls.Where(c => c.Type == ControlTypes.CheckBox).ToList();
            youSent = eventCheckBoxes.Any(c => c.Name == "Event_Envelope_Sent" && c.Selected);
            someoneReceived = eventCheckBoxes.Any(c => c.Name == "Event_Envelope_Received" && c.Selected);
            recipientSigned = eventCheckBoxes.Any(c => c.Name == "Event_Recipient_Signed" && c.Selected);
        }

        public override Task<ActivityDO> Activate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            string errorMessage;
            ActivityIsValid(curActivityDO, out errorMessage);
            //create DocuSign account, publish URL and other user selected options
            bool youSent, someoneReceived, recipientSigned;
            GetUserSelectedEnvelopeEvents(curActivityDO, out youSent, out someoneReceived, out recipientSigned);
            //create or update the DocuSign connect profile configuration
            CreateOrUpdateDocuSignConnectConfiguration(youSent, someoneReceived, recipientSigned);
            return Task.FromResult(curActivityDO);
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

        protected internal override bool ActivityIsValid(ActivityDO curActivityDO, out string errorMessage)
        {
            var errorMessages = new List<string>();
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var configControls = GetConfigurationControls(crateStorage);
                if (configControls == null)
                {
                    errorMessage = "Controls are not configured properly";
                    return false;
                }
                if (!AtLeastOneNotificationIsSelected(configControls, out errorMessage))
                {
                    errorMessages.Add(errorMessage);
                }
                if (EnvelopeConditionIsSelected(configControls, out errorMessage))
                {
                    if (RecipientIsRequired(configControls) && !RecipientIsSet(configControls, out errorMessage))
                    {
                        errorMessages.Add(errorMessage);
                    }
                    if (TemplateIsRequired(configControls))
                    {
                        if (AtLeastOneTemplateExists(configControls, out errorMessage))
                        {
                            if (!TemplateIsSelected(configControls, out errorMessage))
                            {
                                errorMessages.Add(errorMessage);
                            }
                        }
                        else
                        {
                            errorMessages.Add(errorMessage);
                        }
                    }
                }
                else
                {
                    errorMessages.Add(errorMessage);
                }
                errorMessage = string.Join(Environment.NewLine, errorMessages);
            }
            return errorMessages.Count == 0;
        }

        protected override string ActivityUserFriendlyName => "Monitor DocuSign Envelope Activity";

        private bool TemplateIsSelected(StandardConfigurationControlsCM configControls, out string errorMessage)
        {
            var templateList = GetTemplateRadioOption(configControls)
                .Controls
                .OfType<DropDownList>()
                .First();
            var result = !string.IsNullOrEmpty(templateList.selectedKey);
            templateList.ErrorMessage = errorMessage = result ? string.Empty : "Template is not selected";
            return result;
        }
        private bool AtLeastOneTemplateExists(StandardConfigurationControlsCM configControls, out string errorMessage)
        {
            var templateList = GetTemplateRadioOption(configControls)
                .Controls
                .OfType<DropDownList>()
                .First();
            var result = templateList.ListItems.Count > 0;
            templateList.ErrorMessage = errorMessage = result ? string.Empty : "Please link at least one template to your DocuSign account";
            return result;
        }
        private bool TemplateIsRequired(StandardConfigurationControlsCM configControls)
        {
            return GetTemplateRadioOption(configControls).Selected;
        }
        private bool RecipientIsSet(StandardConfigurationControlsCM configControls, out string errorMessage)
        {
            //TODO: set TextBox.ErrorMessage when it will be fixed on front-end side (FR-2586)
            var recipientTextBox = GetRecipientRadioOption(configControls)
                .Controls
                .First();
            var result = !string.IsNullOrEmpty(recipientTextBox.Value);
            GetEnvelopeRadioGroup(configControls).ErrorMessage = errorMessage = result ? string.Empty : "Recipient is not set";
            return result;
        }
        private bool RecipientIsRequired(StandardConfigurationControlsCM configControls)
        {
            return GetRecipientRadioOption(configControls).Selected;
        }
        private bool AtLeastOneNotificationIsSelected(StandardConfigurationControlsCM configControls, out string errorMessage)
        {
            var eventCheckBoxes = configControls.Controls.Where(c => c.Type == ControlTypes.CheckBox).ToArray();
            var anySelectedControl = eventCheckBoxes.Any(c => c.Selected);
            var checkBoxControl = eventCheckBoxes.FirstOrDefault(x => x.Name == "Event_Recipient_Signed");
            errorMessage = anySelectedControl ? string.Empty : "At least one notification checkbox must be checked.";
            if (checkBoxControl != null)
            {
                checkBoxControl.ErrorMessage = errorMessage;
            }
            return string.IsNullOrEmpty(errorMessage);
        }
        private bool EnvelopeConditionIsSelected(StandardConfigurationControlsCM configControls, out string errorMessage)
        {
            var groupControl = configControls.Controls.OfType<RadioButtonGroup>().FirstOrDefault();
            errorMessage = groupControl == null
                               ? "Configuration controls doesn't contain envelope condition selector."
                               : groupControl.Radios.Any(x => x.Selected)
                                    ? string.Empty
                                    : "One option from the radio buttons must be selected.";
            if (groupControl != null)
            {
                groupControl.ErrorMessage = errorMessage;
            }
            return string.IsNullOrEmpty(errorMessage);
        }
        private static RadioButtonGroup GetEnvelopeRadioGroup(StandardConfigurationControlsCM configControls)
        {
            return configControls
                .Controls
                .OfType<RadioButtonGroup>()
                .First();
        }

        private RadioButtonOption GetRecipientRadioOption(StandardConfigurationControlsCM configControls)
        {
            return GetEnvelopeRadioGroup(configControls)
                .Radios
                .First(x => x.Name == "recipient");
        }

        private RadioButtonOption GetTemplateRadioOption(StandardConfigurationControlsCM configControls)
        {
            return GetEnvelopeRadioGroup(configControls)
                .Radios
                .First(x => x.Name == "template");
        }
        /// <summary>
        /// Creates or Updates a Docusign connect configuration named "DocuSignConnectName" for current user
        /// </summary>
        private void CreateOrUpdateDocuSignConnectConfiguration(bool youSent, bool someoneReceived, bool recipientSigned)
        {
            //prepare envelope events based on the input parameters
            var envelopeEvents = new List<string>(3);
            if (youSent)
            {
                envelopeEvents.Add(DocuSignOnEnvelopeSentEvent);
            }
            if (someoneReceived)
            {
                envelopeEvents.Add(DocuSignOnEnvelopeReceivedEvent);
            }
            if (recipientSigned)
            {
                envelopeEvents.Add(DocuSignOnEnvelopeSignedEvent);
            }
            //get existing connect configuration
            DocuSignAccount.CreateOrUpdateDefaultDocuSignConnectConfiguration(string.Join(",", envelopeEvents));
        }

        public override Task<ActivityDO> Deactivate(ActivityDO curActivityDO)
        {
            //get existing DocuSign connect profile
            var docuSignAccount = new DocuSignAccount();
            var existingConfig = GetDocuSignConnectConfiguration(docuSignAccount);

            //if there is a config existing, delete the connect configuration
            if (existingConfig != null)
            {
                docuSignAccount.DeleteDocuSignConnectProfile(existingConfig.connectId);
            }

            return Task.FromResult(curActivityDO);
        }

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);
            //get currently selected option and its value
            string curSelectedOption, curSelectedValue, curSelectedTemplate;
            GetTemplateRecipientPickerValue(curActivityDO, out curSelectedOption, out curSelectedValue, out curSelectedTemplate);
            var envelopeId = string.Empty;
            //retrieve envelope ID based on the selected option and its value
            if (!string.IsNullOrEmpty(curSelectedOption))
            {
                switch (curSelectedOption)
                {
                    case "template":
                        //filter the incoming envelope by template value selected by the user                       
                        var incommingTemplate = GetValueForKey(payloadCrates, "TemplateName");
                        if (incommingTemplate != null)
                        {
                            if (curSelectedTemplate == incommingTemplate)
                            {
                                envelopeId = GetValueForKey(payloadCrates, "EnvelopeId");
                            }
                            else
                            {
                                //this event isn't about us let's stop execution
                                return TerminateHubExecution(payloadCrates);
                            }
                        }
                        break;
                    case "recipient":
                        //filter incoming envelope by recipient email address specified by the user
                        var curRecipientEmail = GetValueForKey(payloadCrates, "RecipientEmail");
                        if (curRecipientEmail != null)
                        {
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
                        }
                        break;
                }
            }

            // Make sure that it exists
            if (string.IsNullOrEmpty(envelopeId))
            {
                await Activate(curActivityDO, authTokenDO);
                return Success(payloadCrates, "Route successfully activated. It will wait and respond to specified DocuSign Event messages");
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
                        Data = "Monitor DocuSign activity successfully recieved an envelope ID " + envelopeId,
                        IsLogged = false
                    }
                }
            };
            
            using (var crateStorage = CrateManager.GetUpdatableStorage(payloadCrates))
            {
                crateStorage.Add(Crate.FromContent("DocuSign Envelope Payload Data", new StandardPayloadDataCM(fields)));
                crateStorage.Add(Crate.FromContent("Log Messages", logMessages));
                if (curSelectedOption == "template")
                {
                    var userDefinedFieldsPayload = _docuSignManager.CreateActivityPayload(curActivityDO, authTokenDO, envelopeId, curSelectedValue);
                    crateStorage.Add(Crate.FromContent("DocuSign Envelope Data", userDefinedFieldsPayload));
                }
            }

            return Success(payloadCrates);
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

            var configurationCrate = PackCrate_ConfigurationControls();
            _docuSignManager.FillDocuSignTemplateSource(configurationCrate, "UpstreamCrate", docuSignAuthDTO);
            var eventFields = CrateManager.CreateDesignTimeFieldsCrate("DocuSign Event Fields", AvailabilityType.RunTime, CreateDocuSignEventFields().ToArray());

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Add(configurationCrate);
                crateStorage.Add(eventFields);

                // Remove previously added crate of "Standard Event Subscriptions" schema
                crateStorage.Remove<EventSubscriptionCM>();
                crateStorage.Add(PackCrate_EventSubscriptions(configurationCrate.Get<StandardConfigurationControlsCM>()));
            }
            return await Task.FromResult(curActivityDO);
        }

        protected override Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //just update the user selected envelope events in the follow up configuration
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                UpdateSelectedEvents(crateStorage);
                string selectedOption, selectedValue, selectedTemplate;
                GetTemplateRecipientPickerValue(curActivityDO, out selectedOption, out selectedValue, out selectedTemplate);
                _docuSignManager.UpdateUserDefinedFields(curActivityDO, authTokenDO, crateStorage, selectedValue);
            }
            return Task.FromResult(curActivityDO);
        }

        /// <summary>
        /// Updates event subscriptions list by user checked check boxes.
        /// </summary>
        /// <remarks>The configuration controls include check boxes used to get the selected DocuSign event subscriptions</remarks>
        private void UpdateSelectedEvents(ICrateStorage storage)
        {
            //get the config controls manifest

            var curConfigControlsCrate = storage.CrateContentsOfType<StandardConfigurationControlsCM>().First();

            //get selected check boxes (i.e. user wanted to subscribe these DocuSign events to monitor for)
            var curSelectedDocuSignEvents =
                curConfigControlsCrate.Controls
                    .Where(configControl => configControl.Type.Equals(ControlTypes.CheckBox) && configControl.Selected && configControl.Name.StartsWith("Event_"))
                    .Select(checkBox => checkBox.Name.Substring("Event_".Length).Replace("_", "")).ToList();

            if (curSelectedDocuSignEvents.Any(e => e == "RecipientSigned"))
            {
                if (curSelectedDocuSignEvents.Any(e => e != "RecipientCompleted"))
                {
                    curSelectedDocuSignEvents.Add("RecipientCompleted");
                }
            }
            else
                curSelectedDocuSignEvents.Remove("RecipientCompleted");

            //create standard event subscription crate with user selected DocuSign events
            var curEventSubscriptionCrate = CrateManager.CreateStandardEventSubscriptionsCrate("Standard Event Subscriptions", "DocuSign",
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
                    if (eventCheckBox.Name.Equals("Event_Recipient_Signed", StringComparison.InvariantCultureIgnoreCase))
                    {
                        subscriptions.Add("RecipientCompleted");
                    }
                }
            }

            return CrateManager.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                "DocuSign",
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
                                Events = new List<ControlEvent> {new ControlEvent("onChange", "requestConfig")},
                                ShowDocumentation = ActivityResponseDTO.CreateDocumentationResponse("Minicon", "ExplainMonitoring")
                            }
                        }
                    }
                }
            };

            return templateRecipientPicker;
        }

    }
}
