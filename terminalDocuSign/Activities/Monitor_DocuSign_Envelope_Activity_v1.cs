using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Validations;
using DocuSign.eSign.Api;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Managers;
using terminalDocuSign.Activities;
using terminalDocuSign.Services.New_Api;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalDocuSign.Actions
{
    public class Monitor_DocuSign_Envelope_Activity_v1 : BaseDocuSignActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Version = "1",
            Name = "Monitor_DocuSign_Envelope_Activity",
            Label = "Monitor DocuSign Envelope Activity",
            Category = ActivityCategory.Monitors,
            NeedsAuthentication = true,
            MinPaneWidth = 380,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string RecipientSignedEventName = "RecipientSigned";
        private const string RecipientCompletedEventName = "RecipientCompleted";
        private const string EnvelopeSentEventname = "EnvelopeSent";
        private const string EnvelopeRecievedEventName = "EnvelopeReceived";

        private const string AllFieldsCrateName = "DocuSign Envelope Fields";

        public Monitor_DocuSign_Envelope_Activity_v1(ICrateManager crateManager, IDocuSignManager docuSignManager) 
            : base(crateManager, docuSignManager)
        {
        }

        private void GetTemplateRecipientPickerValue(out string selectedOption, out string selectedValue, out string selectedTemplate)
        {
            ActivityUi activityUi = ConfigurationControls;
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

        protected override Task Validate()
        {
            ActivityUi activityUi = ConfigurationControls;
            if (activityUi == null)
            {
                ValidationManager.SetError(DocuSignValidationUtils.ControlsAreNotConfiguredErrorMessage);
            }

            if (!AtLeastOneNotificationIsSelected(activityUi))
            {
                ValidationManager.SetError("At least one notification option must be selected", activityUi.EnvelopeSignedOption);
            }

            if (!EnvelopeConditionIsSelected(activityUi))
            {
                ValidationManager.SetError("At least one envelope option must be selected", activityUi.TemplateRecipientOptionSelector);
            }

            if (RecipientIsRequired(activityUi))
            {
                if (DocuSignValidationUtils.ValueIsSet(activityUi.Recipient))
                {
                    ValidationManager.ValidateEmail(activityUi.Recipient, DocuSignValidationUtils.RecipientIsNotValidErrorMessage);
                }
                else
                {
                    ValidationManager.SetError(DocuSignValidationUtils.RecipientIsNotSpecifiedErrorMessage, activityUi.Recipient);
                }
            }

            if (TemplateIsRequired(activityUi))
            {
                ValidationManager.ValidateTemplateList(activityUi.TemplateList);
            }

            return Task.FromResult(0);
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
        
        protected override async Task RunDS()
        {
            DocuSignEnvelopeCM_v2 envelopeStatus = null;
            var eventCrate = Payload.CratesOfType<EventReportCM>().FirstOrDefault()?.Get<EventReportCM>()?.EventPayload;
            if (eventCrate != null)
                envelopeStatus = eventCrate.CrateContentsOfType<DocuSignEnvelopeCM_v2>().SingleOrDefault();

            if (envelopeStatus == null)
            {
                TerminateHubExecution("Evelope was not found in the payload.");
                return;
            }

            //Create run-time fields
            var eventFields = CreateDocuSignEventFields(envelopeStatus);

            //get currently selected option and its value
            string curSelectedOption, curSelectedValue, curSelectedTemplate;
            GetTemplateRecipientPickerValue(out curSelectedOption, out curSelectedValue, out curSelectedTemplate);
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
                            var config = manager.SetUp(AuthorizationToken);
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
                            TerminateHubExecution();
                            return;
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
                                TerminateHubExecution();
                                return;
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

            List<FieldDTO> allFields = new List<FieldDTO>();
            allFields.AddRange(eventFields);
            //crateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Payload Data", new StandardPayloadDataCM(fields)));
            Payload.Add(Crate.FromContent("Log Messages", logMessages));
            if (curSelectedOption == "template")
            {
                allFields.AddRange(GetEnvelopeData(envelopeId, null));
            }
            // TODO: This is probably obsolete crate, however lookup of that particular crate is hardcoded in QueryFr8Warehouse_v1#GetCurrentEnvelopeId.
            //          This was possibly required by Generate_DocuSign_Report.
            Payload.Add(Crate.FromContent("DocuSign Envelope Payload Data", new StandardPayloadDataCM(eventFields)));
            // Crate that should be used, since it is base on CrateDescriptionCM.
            Payload.Add(CrateManager.CreateDesignTimeFieldsCrate(AllFieldsCrateName, AvailabilityType.RunTime, allFields.ToArray()));
            Success();
        }

        protected override async Task InitializeDS()
        {

            var controlsCrate = PackControls(CreateActivityUi());
            FillDocuSignTemplateSource(controlsCrate, "UpstreamCrate");
            //var eventFields = CrateManager.CreateDesignTimeFieldsCrate("DocuSign Event Fields", AvailabilityType.RunTime, CreateDocuSignEventFields().ToArray());
            Storage.Add(controlsCrate);
            //crateStorage.Add(eventFields);
            // Remove previously added crate of "Standard Event Subscriptions" schema
            Storage.Remove<EventSubscriptionCM>();
            Storage.Add(PackEventSubscriptionsCrate(controlsCrate.Get<StandardConfigurationControlsCM>()));
            // Add Crate Description
            Storage.Add(GetAvailableRunTimeTableCrate());
        }

        protected override Task FollowUpDS()
        {
            //just update the user selected envelope events in the follow up configuration
            var allFields = CreateDocuSignEventFields(null, AllFieldsCrateName);
            UpdateSelectedEvents(Storage);
            string selectedOption, selectedValue, selectedTemplate;
            GetTemplateRecipientPickerValue(out selectedOption, out selectedValue, out selectedTemplate);
            if (selectedOption == "template")
            {
                allFields.AddRange(GetTemplateUserDefinedFields(selectedValue, null));
            }
            // Update all fields crate
            Storage.RemoveByLabel(AllFieldsCrateName);
            Storage.Add(CrateManager.CreateDesignTimeFieldsCrate(AllFieldsCrateName, AvailabilityType.RunTime, allFields.ToArray()));
            return Task.FromResult(0);
        }

        private Crate GetAvailableRunTimeTableCrate()
        {
            var availableRunTimeCrates = Crate.FromContent("Available Run Time Crates", new CrateDescriptionCM(
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
