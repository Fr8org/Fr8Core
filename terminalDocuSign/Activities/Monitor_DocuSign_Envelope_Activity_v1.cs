using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Data.Validations;
using DocuSign.eSign.Api;
using Hub.Managers;
using terminalDocuSign.Services.New_Api;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalDocuSign.Activities
{
    public class Monitor_DocuSign_Envelope_Activity_v1 : BaseDocuSignActivity
    {

        private const string RecipientSignedEventName = "RecipientSigned";
        private const string RecipientCompletedEventName = "RecipientCompleted";
        private const string EnvelopeSentEventname = "EnvelopeSent";
        private const string EnvelopeRecievedEventName = "EnvelopeReceived";

        private const string AllFieldsCrateName = "DocuSign Envelope Fields";

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return curActivityDO;
            }

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
            ActivityUi activityUi = CrateManager.GetStorage(curActivityDO).FirstCrate<StandardConfigurationControlsCM>(x => x.Label == "Configuration_Controls").Content;
            selectedOption = string.Empty;
            selectedValue = string.Empty;
            selectedTemplate = string.Empty;
            if (activityUi.BasedOnTemplateOption.Selected)
            {
                selectedOption = activityUi.BasedOnTemplateOption.Name;
                selectedTemplate = activityUi.TemplateList.selectedKey;
                selectedValue = activityUi.TemplateList.Value;
            }
            else if (activityUi.SentToRecipientOption.Selected)
            {
                selectedOption = activityUi.SentToRecipientOption.Name;
                selectedValue = activityUi.Recipient.Value;
            }
        }

        private DocuSignEvents GetUserSelectedEnvelopeEvents(ActivityDO curActivityDO)
        {
            ActivityUi activityUi = GetConfigurationControls(curActivityDO);
            return new DocuSignEvents
            {
                EnvelopeSent = activityUi?.EnvelopeSentOption?.Selected ?? false,
                EnvelopRecieved = activityUi?.EnvelopeRecievedOption?.Selected ?? false,
                EnvelopeSigned = activityUi?.EnvelopeSignedOption?.Selected ?? false
            };
        }

        public override Task<ActivityDO> Activate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            return Task.FromResult(curActivityDO);
        }

        protected internal override ValidationResult ValidateActivityInternal(ActivityDO curActivityDO)
        {
            var errorMessages = new List<string>();
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                ActivityUi activityUi = GetConfigurationControls(crateStorage);
                if (activityUi == null)
                {
                    return new ValidationResult(DocuSignValidationUtils.ControlsAreNotConfiguredErrorMessage);
                }
                errorMessages.Add(activityUi.EnvelopeSignedOption.ErrorMessage
                                  = AtLeastOneNotificationIsSelected(activityUi)
                                        ? string.Empty
                                        : "At least one notification option must be selected");

                errorMessages.Add(activityUi.TemplateRecipientOptionSelector.ErrorMessage
                                  = EnvelopeConditionIsSelected(activityUi)
                                        ? string.Empty
                                        : "At least one envelope option must be selected");

                errorMessages.Add(activityUi.Recipient.ErrorMessage
                    = RecipientIsRequired(activityUi)
                        ? DocuSignValidationUtils.ValueIsSet(activityUi.Recipient)
                            ? activityUi.Recipient.Value.IsValidEmailAddress()
                                ? string.Empty
                                : DocuSignValidationUtils.RecipientIsNotValidErrorMessage
                            : DocuSignValidationUtils.RecipientIsNotSpecifiedErrorMessage
                        : string.Empty);

                errorMessages.Add(activityUi.TemplateList.ErrorMessage
                                  = TemplateIsRequired(activityUi)
                                        ? DocuSignValidationUtils.AtLeastOneItemExists(activityUi.TemplateList)
                                              ? DocuSignValidationUtils.ItemIsSelected(activityUi.TemplateList)
                                                    ? string.Empty
                                                    : DocuSignValidationUtils.TemplateIsNotSelectedErrorMessage
                                              : DocuSignValidationUtils.NoTemplateExistsErrorMessage
                                        : string.Empty);
            }
            errorMessages.RemoveAll(string.IsNullOrEmpty);
            return errorMessages.Count == 0 ? ValidationResult.Success : new ValidationResult(string.Join(Environment.NewLine, errorMessages));
        }

        protected override string ActivityUserFriendlyName => "Monitor DocuSign Envelope Activity";

        private bool TemplateIsRequired(ActivityUi activityUi)
        {
            return activityUi.BasedOnTemplateOption.Selected;
        }
        private bool RecipientIsRequired(ActivityUi activityUi)
        {
            return activityUi.SentToRecipientOption.Selected;
        }
        private bool AtLeastOneNotificationIsSelected(ActivityUi activityUi)
        {
            return activityUi.EnvelopeRecievedOption.Selected
                   || activityUi.EnvelopeSentOption.Selected
                   || activityUi.EnvelopeSignedOption.Selected;
        }
        private bool EnvelopeConditionIsSelected(ActivityUi activityUi)
        {
            return activityUi.SentToRecipientOption.Selected || activityUi.BasedOnTemplateOption.Selected;
        }

        public override Task<ActivityDO> Deactivate(ActivityDO curActivityDO)
        {
            return Task.FromResult(curActivityDO);
        }

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);

            DocuSignEnvelopeCM_v2 envelopeStatus = null;

            var eventCrate = CrateManager.FromDto(payload.CrateStorage).CratesOfType<EventReportCM>().FirstOrDefault()?.Get<EventReportCM>()?.EventPayload;
            if (eventCrate != null)
                envelopeStatus = eventCrate.CrateContentsOfType<DocuSignEnvelopeCM_v2>().SingleOrDefault();

            if (envelopeStatus == null)
            {
                await Activate(curActivityDO, authTokenDO);
                return TerminateHubExecution(payload, "Plan successfully activated. It will wait and respond to specified DocuSign Event messages");
            }

            //Create run-time fields
            var eventFields = CreateDocuSignEventFields(envelopeStatus);

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
                        var incomingTemplate = string.Join(",", envelopeStatus.Templates.Select(t => t.Name).ToArray());

                        //Dirty quick fix for FR-2858
                        if (string.IsNullOrEmpty(incomingTemplate))
                        {
                            var manager = new DocuSignManager();
                            var config = manager.SetUp(authTokenDO);
                            TemplatesApi api = new TemplatesApi(config.Configuration);
                            var templateslist = api.ListTemplates(config.AccountId);
                            if (templateslist.TotalSetSize == "0")
                                break;
                            incomingTemplate = string.Join(",", templateslist.EnvelopeTemplates.Select(a => a.Name).ToArray());
                        }

                        if (incomingTemplate.Contains(curSelectedTemplate, StringComparison.InvariantCultureIgnoreCase))
                        {
                            envelopeId = envelopeStatus.EnvelopeId;
                        }
                        else
                        {
                            //this event isn't about us let's stop execution
                            return TerminateHubExecution(payload);
                        }

                        break;
                    case "recipient":

                        string envelopeCurentRecipientEmail = eventFields.Where(a => a.Key == "CurrentRecipientEmail").FirstOrDefault().Value;
                        //filter incoming envelope by recipient email address specified by the user
                        if (envelopeCurentRecipientEmail != null)
                        {
                            //if the incoming envelope's recipient is user specified one, get the envelope ID
                            if (envelopeCurentRecipientEmail.Contains(curSelectedValue, StringComparison.InvariantCultureIgnoreCase))
                            {
                                envelopeId = envelopeStatus.EnvelopeId;
                            }
                            else
                            {
                                //this event isn't about us let's stop execution
                                return TerminateHubExecution(payload);
                            }
                        }
                        break;
                }
            }





            //Create log message
            var logMessages = new StandardLoggingCM
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

            using (var crateStorage = CrateManager.GetUpdatableStorage(payload))
            {
                List<FieldDTO> allFields = new List<FieldDTO>();
                allFields.AddRange(eventFields);

                //crateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Payload Data", new StandardPayloadDataCM(fields)));
                crateStorage.Add(Data.Crates.Crate.FromContent("Log Messages", logMessages));
                if (curSelectedOption == "template")
                {
                    allFields.AddRange(GetEnvelopeData(authTokenDO, envelopeId, null));
                }

                // TODO: This is probably obsolete crate, however lookup of that particular crate is hardcoded in QueryFr8Warehouse_v1#GetCurrentEnvelopeId.
                //          This was possibly required by Generate_DocuSign_Report.
                crateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Payload Data", new StandardPayloadDataCM(eventFields)));

                // Crate that should be used, since it is base on CrateDescriptionCM.
                crateStorage.Add(Crate.CreateDesignTimeFieldsCrate(AllFieldsCrateName, AvailabilityType.RunTime, allFields.ToArray()));
            }

            return Success(payload);
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {

            var controlsCrate = PackControls(CreateActivityUi());
            FillDocuSignTemplateSource(controlsCrate, "UpstreamCrate", authTokenDO);
            //var eventFields = CrateManager.CreateDesignTimeFieldsCrate("DocuSign Event Fields", AvailabilityType.RunTime, CreateDocuSignEventFields().ToArray());

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Add(controlsCrate);
                //crateStorage.Add(eventFields);

                // Remove previously added crate of "Standard Event Subscriptions" schema
                crateStorage.Remove<EventSubscriptionCM>();
                crateStorage.Add(PackEventSubscriptionsCrate(controlsCrate.Get<StandardConfigurationControlsCM>()));

                // Add Crate Description
                crateStorage.Add(GetAvailableRunTimeTableCrate());

            }
            return await Task.FromResult(curActivityDO);
        }

        protected override Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //just update the user selected envelope events in the follow up configuration
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var allFields = CreateDocuSignEventFields(null);
                UpdateSelectedEvents(crateStorage);
                string selectedOption, selectedValue, selectedTemplate;
                GetTemplateRecipientPickerValue(curActivityDO, out selectedOption, out selectedValue, out selectedTemplate);
                if (selectedOption == "template")
                {
                    allFields.AddRange(GetTemplateUserDefinedFields(authTokenDO, selectedValue, null));
                }

                // Update all fields crate
                crateStorage.RemoveByLabel(AllFieldsCrateName);
                crateStorage.Add(Crate.CreateDesignTimeFieldsCrate(AllFieldsCrateName, AvailabilityType.RunTime, allFields.ToArray()));

            }
            return Task.FromResult(curActivityDO);
        }

        private Crate GetAvailableRunTimeTableCrate()
        {
            var availableRunTimeCrates = Data.Crates.Crate.FromContent("Available Run Time Crates", new CrateDescriptionCM(
                    new CrateDescriptionDTO
                    {
                        ManifestType = MT.FieldDescription.GetEnumDisplayName(),
                        Label = AllFieldsCrateName,
                        ManifestId = (int)MT.FieldDescription,
                        ProducedBy = "Monitor_DocuSign_Envelope_Activity_v1"
                    }), AvailabilityType.RunTime);
            return availableRunTimeCrates;
        }


        /// <summary>
        /// Updates event subscriptions list by user checked check boxes.
        /// </summary>
        /// <remarks>The configuration controls include check boxes used to get the selected DocuSign event subscriptions</remarks>
        private void UpdateSelectedEvents(ICrateStorage storage)
        {
            ActivityUi activityUi = storage.CrateContentsOfType<StandardConfigurationControlsCM>().First();

            //get selected check boxes (i.e. user wanted to subscribe these DocuSign events to monitor for)
            var curSelectedDocuSignEvents = new List<string>
                                            {
                                                activityUi.EnvelopeSentOption.Selected ? activityUi.EnvelopeSentOption.Name : string.Empty,
                                                activityUi.EnvelopeRecievedOption.Selected ? activityUi.EnvelopeRecievedOption.Name : string.Empty,
                                                activityUi.EnvelopeSignedOption.Selected ? activityUi.EnvelopeSignedOption.Name : string.Empty
                                            };
            if (curSelectedDocuSignEvents.Contains(RecipientSignedEventName))
            {
                if (!curSelectedDocuSignEvents.Contains(RecipientCompletedEventName))
                {
                    curSelectedDocuSignEvents.Add(RecipientCompletedEventName);
                }
            }
            else
            {
                curSelectedDocuSignEvents.Remove(RecipientCompletedEventName);
            }

            //create standard event subscription crate with user selected DocuSign events
            var curEventSubscriptionCrate = CrateManager.CreateStandardEventSubscriptionsCrate("Standard Event Subscriptions", "DocuSign",
                curSelectedDocuSignEvents.Where(x => !string.IsNullOrEmpty(x)).ToArray());

            storage.Remove<EventSubscriptionCM>();
            storage.Add(curEventSubscriptionCrate);
        }

        private Crate PackEventSubscriptionsCrate(StandardConfigurationControlsCM configurationFields)
        {
            var subscriptions = new List<string>();
            ActivityUi activityUi = configurationFields;
            if (activityUi.EnvelopeSentOption.Selected)
            {
                subscriptions.Add(EnvelopeSentEventname);
            }
            if (activityUi.EnvelopeRecievedOption.Selected)
            {
                subscriptions.Add(EnvelopeRecievedEventName);
            }
            if (activityUi.EnvelopeSignedOption.Selected)
            {
                subscriptions.Add(RecipientSignedEventName);
                subscriptions.Add(RecipientCompletedEventName);
            }
            return CrateManager.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                "DocuSign",
                subscriptions.ToArray());
        }

        private ActivityUi CreateActivityUi()
        {
            var result = new ActivityUi
            {
                ActivityDescription = new TextArea
                {
                    IsReadOnly = true,
                    Label = "",
                    Value = "<p>Process incoming DocuSign Envelope notifications if the following are true:</p>"
                },
                EnvelopeSentOption = new CheckBox
                {
                    Label = "You sent a DocuSign Envelope",
                    Name = EnvelopeSentEventname,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                },
                EnvelopeRecievedOption = new CheckBox
                {
                    Label = "Someone received an Envelope you sent",
                    Name = EnvelopeRecievedEventName,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                },
                EnvelopeSignedOption = new CheckBox
                {
                    Label = "One of your Recipients signed an Envelope",
                    Name = RecipientSignedEventName,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                },
                Recipient = new TextBox
                {
                    Label = "",
                    Name = "RecipientValue",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                },
                SentToRecipientOption = new RadioButtonOption
                {
                    Selected = false,
                    Name = "recipient",
                    Value = "Was sent to a specific recipient"
                },
                TemplateList = new DropDownList
                {
                    Label = "",
                    Name = "UpstreamCrate",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    ShowDocumentation = ActivityResponseDTO.CreateDocumentationResponse("Minicon", "ExplainMonitoring")
                },
                BasedOnTemplateOption = new RadioButtonOption
                {
                    Selected = false,
                    Name = "template",
                    Value = "Was based on a specific template"
                },
                TemplateRecipientOptionSelector = new RadioButtonGroup
                {
                    Label = "The envelope:",
                    GroupName = "TemplateRecipientPicker",
                    Name = "TemplateRecipientPicker",
                    Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
                }
            };
            result.BasedOnTemplateOption.Controls = new List<ControlDefinitionDTO> { result.TemplateList };
            result.SentToRecipientOption.Controls = new List<ControlDefinitionDTO> { result.Recipient };
            result.TemplateRecipientOptionSelector.Radios = new List<RadioButtonOption> { result.SentToRecipientOption, result.BasedOnTemplateOption };
            return result;
        }

        private struct DocuSignEvents
        {
            public bool EnvelopeSent { get; set; }
            public bool EnvelopRecieved { get; set; }
            public bool EnvelopeSigned { get; set; }
        }

        private class ActivityUi
        {
            public TextArea ActivityDescription { get; set; }
            public CheckBox EnvelopeSentOption { get; set; }
            public CheckBox EnvelopeRecievedOption { get; set; }
            public CheckBox EnvelopeSignedOption { get; set; }
            public RadioButtonGroup TemplateRecipientOptionSelector { get; set; }
            public RadioButtonOption BasedOnTemplateOption { get; set; }
            public DropDownList TemplateList { get; set; }
            public RadioButtonOption SentToRecipientOption { get; set; }
            public TextBox Recipient { get; set; }

            public static implicit operator ActivityUi(StandardConfigurationControlsCM controlsManifest)
            {
                if (controlsManifest == null)
                {
                    return null;
                }
                try
                {
                    var result = new ActivityUi
                    {
                        ActivityDescription = (TextArea)controlsManifest.Controls[0],
                        EnvelopeSentOption = (CheckBox)controlsManifest.Controls[1],
                        EnvelopeRecievedOption = (CheckBox)controlsManifest.Controls[2],
                        EnvelopeSignedOption = (CheckBox)controlsManifest.Controls[3],
                        TemplateRecipientOptionSelector = (RadioButtonGroup)controlsManifest.Controls[4]
                    };
                    result.SentToRecipientOption = result.TemplateRecipientOptionSelector.Radios[0];
                    result.Recipient = (TextBox)result.SentToRecipientOption.Controls[0];
                    result.BasedOnTemplateOption = result.TemplateRecipientOptionSelector.Radios[1];
                    result.TemplateList = (DropDownList)result.BasedOnTemplateOption.Controls[0];
                    return result;
                }
                catch
                {
                    return null;
                }
            }

            public static implicit operator StandardConfigurationControlsCM(ActivityUi activityUi)
            {
                if (activityUi == null)
                {
                    return null;
                }
                return new StandardConfigurationControlsCM(activityUi.ActivityDescription,
                                                           activityUi.EnvelopeSentOption,
                                                           activityUi.EnvelopeRecievedOption,
                                                           activityUi.EnvelopeSignedOption,
                                                           activityUi.TemplateRecipientOptionSelector);
            }
        }
    }
}
