using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using terminalGoogle.Services;
using StructureMap;
using terminalGoogle.Interfaces;

namespace terminalGoogle.Actions
{
    public class Monitor_Form_Responses_v1 : BaseGoogleTerminalActivity<Monitor_Form_Responses_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList FormsList { get; set; }

            public ActivityUi()
            {
                FormsList = new DropDownList()
                {
                    Label = "Select Google Form",
                    Name = "Selected_Google_Form",
                    Required = true,
                    Source = null,
                    Events = { ControlEvent.RequestConfig }
                };
                Controls.Add(FormsList);
            }
        }
        private readonly GoogleDrive _googleDrive;
        private readonly GoogleAppScript _googleAppScript;

        private readonly IGoogleIntegration _googleIntegration;
        private const string ConfigurationCrateLabel = "Selected_Google_Form";
        private const string RunTimeCrateLabel = "Google Form Payload Data";
        private const string EventSubscriptionsCrateLabel = "Standard Event Subscriptions";
        private FieldDTO SelectedForm
        {
            get
            {
                var storedValues = CurrentActivityStorage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == ConfigurationCrateLabel)?.Content;
                return storedValues?.Fields.First();
            }
            set
            {
                if (value == null)
                {
                    CurrentActivityStorage.RemoveByLabel(ConfigurationCrateLabel);
                    return;
                }
                value.Availability = AvailabilityType.Configuration;
                var newValues = Crate.FromContent(ConfigurationCrateLabel, new FieldDescriptionsCM(value), AvailabilityType.Configuration);
                CurrentActivityStorage.ReplaceByLabel(newValues);
            }
        }
        public Monitor_Form_Responses_v1()
        {
            _googleDrive = new GoogleDrive();
            _googleAppScript = new GoogleAppScript();
            _googleIntegration = ObjectFactory.GetInstance<IGoogleIntegration>();
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            var googleAuth = GetGoogleAuthToken();
            var forms = await _googleDrive.GetGoogleForms(googleAuth);
            ConfigurationControls.FormsList.ListItems = forms
                .Select(x => new ListItem { Key = x.Value, Value = x.Key })
                .ToList();
            CurrentActivityStorage.Add(CreateEventSubscriptionCrate());
            runtimeCrateManager.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel);
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            var googleAuth = GetGoogleAuthToken();
            var forms = await _googleDrive.GetGoogleForms(googleAuth);
            ConfigurationControls.FormsList.ListItems = forms
                .Select(x => new ListItem { Key = x.Value, Value = x.Key })
                .ToList();
            var selectedSpreadsheet = ConfigurationControls.FormsList.selectedKey;
            if (!string.IsNullOrEmpty(selectedSpreadsheet))
            {
                bool any = ConfigurationControls.FormsList.ListItems.Any(x => x.Key == selectedSpreadsheet);
                if (!any)
                {
                    ConfigurationControls.FormsList.selectedKey = null;
                    ConfigurationControls.FormsList.Value = null;
                }
            }
            if (string.IsNullOrEmpty(ConfigurationControls.FormsList.selectedKey))
                SelectedForm = null;
            runtimeCrateManager.ClearAvailableCrates();
            runtimeCrateManager.MarkAvailableAtRuntime<StandardPayloadDataCM>(RunTimeCrateLabel)
                .AddField("Full Name")
                .AddField("TR ID")
                .AddField("Email Address")
                .AddField("Period of Availability");
        }

        protected override async Task Activate()
        {
            var googleAuth = GetGoogleAuthToken();
            //get form id
            var googleFormControl = ConfigurationControls.FormsList;
            var formId = googleFormControl.Value;
            if (string.IsNullOrEmpty(formId))
                throw new ArgumentNullException("Google Form selected is empty. Please select google form to receive.");

            bool triggerEvent = false;
            try
            {
                triggerEvent = await _googleDrive.CreateFr8TriggerForDocument(googleAuth, formId, CurrentFr8UserEmail);
            }
            finally
            {
                if (!triggerEvent)
                {
                    //in case of fail as a backup plan use old manual script notification
                    var scriptUrl = await _googleDrive.CreateManualFr8TriggerForDocument(googleAuth, formId);
                    await HubCommunicator.NotifyUser(new TerminalNotificationDTO
                    {
                        Type = "Success",
                        ActivityName = "Monitor_Form_Responses",
                        ActivityVersion = "1",
                        TerminalName = "terminalGoogle",
                        TerminalVersion = "1",
                        Message = "You need to create fr8 trigger on current form please go to this url and run Initialize function manually. Ignore this message if you completed this step before. " + scriptUrl,
                        Subject = "Trigger creation URL"
                    }, CurrentFr8UserId);
                }
            }
        }

        protected override Task RunCurrentActivity()
        {
            var selectedForm = ConfigurationControls.FormsList.Value;
            if (string.IsNullOrEmpty(selectedForm))
                throw new ActivityExecutionException("Form is not selected", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            var payloadFields = ExtractPayloadFields(CurrentPayloadStorage);
            // once we activate the plan we run it. When we run the plan manualy there is no payload with event data. 
            // Just return Success as a quick fix to avoid "Plan Failed" message.
            if (payloadFields == null)
            {
                RequestHubExecutionTermination();
                return Task.FromResult(0);
            }
            var formResponseFields = CreatePayloadFormResponseFields(payloadFields);

            // once we activate the plan we run it. When we run the plan manualy there is no payload with event data. 
            // Just return Success as a quick fix to avoid "Plan Failed" message.
            if (formResponseFields == null)
            {
                RequestHubExecutionTermination();
                return Task.FromResult(0); ;
            }
            CurrentPayloadStorage.Add(Crate.FromContent(RunTimeCrateLabel, new StandardPayloadDataCM(formResponseFields)));
            return Task.FromResult(0);
        }

        private Crate CreateEventSubscriptionCrate()
        {
            var subscriptions = new string[] {
                "Google Form Response"
            };

            return CrateManager.CreateStandardEventSubscriptionsCrate(
                EventSubscriptionsCrateLabel,
                "Google",
                subscriptions.ToArray()
                );
        }

        private List<FieldDTO> CreatePayloadFormResponseFields(List<FieldDTO> payloadfields)
        {
            List<FieldDTO> formFieldResponse = new List<FieldDTO>();
            string[] formresponses = payloadfields.FirstOrDefault(w => w.Key == "response").Value.Split(new char[] { '&' });

            if (formresponses.Length > 0)
            {
                formresponses[formresponses.Length - 1] = formresponses[formresponses.Length - 1].TrimEnd(new char[] { '&' });

                foreach (var response in formresponses)
                {
                    string[] itemResponse = response.Split(new char[] { '=' });

                    if (itemResponse.Length >= 2)
                    {
                        formFieldResponse.Add(new FieldDTO() { Key = itemResponse[0], Value = itemResponse[1] });
                    }
                }
            }
            else
            {
                throw new ArgumentNullException("No payload fields extracted");
            }

            return formFieldResponse;
        }

        private List<FieldDTO> ExtractPayloadFields(ICrateStorage currentPayload)
        {
            var eventReportMS = currentPayload.CrateContentsOfType<EventReportCM>().SingleOrDefault();
            if (eventReportMS == null)
                return null;

            var eventFieldsCrate = eventReportMS.EventPayload.SingleOrDefault();
            if (eventFieldsCrate == null)
                return null;

            return eventReportMS.EventPayload.CrateContentsOfType<StandardPayloadDataCM>().SelectMany(x => x.AllValues()).ToList();
        }
    }

    public class EnumerateFormFieldsResponseItem
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("id")]
        public object Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
    }
}