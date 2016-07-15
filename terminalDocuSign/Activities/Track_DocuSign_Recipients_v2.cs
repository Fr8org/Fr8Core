using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using StructureMap;
using Data.Repositories.MultiTenant;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Models;
using Fr8.TerminalBase.Services;
using Hub.Services.MT;
using Newtonsoft.Json;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Activities
{
    public class Track_DocuSign_Recipients_v2 : DocuSignActivity<Track_DocuSign_Recipients_v2.ActivityUi>
    {
        public static readonly ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("54fdef85-e47d-4762-ba2f-3eae39cd1e9b"),
            Name = "Track_DocuSign_Recipients",
            Label = "Track DocuSign Recipients",
            Version = "2",
            Category = ActivityCategory.Solution,
            NeedsAuthentication = true,
            MinPaneWidth = 380,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[] { ActivityCategories.Solution }
        };
        public class ActivityUi : StandardConfigurationControlsCM
        {
            private const string TakenDeliveryEnvelopeOption = "Taken Delivery";

            private const string EnvelopeSignedEnvelopeOption = "Signed";

            public RadioButtonGroup EnvelopeTypeSelectionGroup { get; set; }

            public RadioButtonOption SentToSpecificRecipientOption { get; set; }

            public TextBox SpecificRecipientEmailText { get; set; }

            public RadioButtonOption BasedOnTemplateOption { get; set; }

            public DropDownList TemplateSelector { get; set; }

            public Duration TimePeriod { get; set; }

            public DropDownList RecipientEventSelector { get; set; }

            public DropDownList NotifierSelector { get; set; }

            public Button BuildSolutionButton { get; set; }
            
            public ActivityUi()
            {
                SpecificRecipientEmailText = new TextBox { Name = nameof(SpecificRecipientEmailText) };
                SentToSpecificRecipientOption = new RadioButtonOption
                {
                    Name = nameof(SentToSpecificRecipientOption),
                    Value = "Envelopes sent to a specific recipient",
                    Selected = true,
                    Controls = new List<ControlDefinitionDTO> { SpecificRecipientEmailText }
                };
                TemplateSelector = new DropDownList { Name = nameof(TemplateSelector) };
                BasedOnTemplateOption = new RadioButtonOption
                {
                    Name = nameof(BasedOnTemplateOption),
                    Value = "Envelopes based on a specific template",
                    Controls = new List<ControlDefinitionDTO> { TemplateSelector }
                };
                EnvelopeTypeSelectionGroup = new RadioButtonGroup
                {
                    Name = nameof(EnvelopeTypeSelectionGroup),
                    Label = "Track which Envelopes?",
                    Radios = new List<RadioButtonOption>
                    {
                        SentToSpecificRecipientOption,
                        BasedOnTemplateOption
                    }
                };
                TimePeriod = new Duration
                {
                    Label = "After you send a Tracked Envelope, Fr8 will wait.",
                    InnerLabel = "Wait this long:",
                    Name = nameof(TimePeriod)
                };
                RecipientEventSelector = new DropDownList
                {
                    Label = "Then Fr8 will notify you if a recipient has not",
                    Name = nameof(RecipientEventSelector),
                    ListItems = new List<ListItem>
                                {
                                    new ListItem { Key = "Taken Delivery", Value = TakenDeliveryEnvelopeOption },
                                    new ListItem { Key = "Signed Envelope", Value = EnvelopeSignedEnvelopeOption }
                                }
                };
                NotifierSelector = new DropDownList
                {
                    Name = nameof(NotifierSelector),
                    Label = "How would you like to be notified?"
                };
                BuildSolutionButton = new Button
                {
                    Name = nameof(BuildSolutionButton),
                    Label = "Build Solution",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfigOnClick }
                };
                Controls.Add(EnvelopeTypeSelectionGroup);
                Controls.Add(TimePeriod);
                Controls.Add(RecipientEventSelector);
                Controls.Add(NotifierSelector);
                Controls.Add(BuildSolutionButton);
            }

            public bool IsTakenDeliveryOptionSelected => RecipientEventSelector.Value == TakenDeliveryEnvelopeOption;

            public bool IsSignedEnvelopeOptionSelected => RecipientEventSelector.Value == EnvelopeSignedEnvelopeOption;
        }

        private const string SolutionName = "Track DocuSign Recipients";

        private const double SolutionVersion = 2.0;

        private const string TerminalName = "DocuSign";

        private const string SolutionBody = @"<p>Link your important outgoing envelopes to Fr8's powerful notification activities, 
                                            which allow you to receive SMS notices, emails, or receive posts to popular tracking systems like Slack and Yammer. 
                                            Get notified when recipients take too long to sign!</p>";

        private const string MessageBody = @"Fr8 Alert: The DocuSign Envelope [TemplateName] has not been [ActionBeingTracked] by [RecipientUserName] at [RecipientEmail]. You had requested a notification after a delay of [DelayTime].";

        private const string RuntimeCrateLabel = "Track Recipient Properties";

        private const string ActionBeingTrackedProperty = "ActionBeingTracked";

        private const string DelayTimeProperty = "DelayTime";

        private const string NotificationMessageLabel = "NotificationMessage";

        private readonly IUnitOfWorkFactory _uowFactory;

        private readonly IConfigRepository _configRepository;

        public Track_DocuSign_Recipients_v2(
            ICrateManager crateManager, 
            IDocuSignManager docuSignManager, 
            IUnitOfWorkFactory uowFactory,
            IConfigRepository configRepository)
            : base(crateManager, docuSignManager)
        {
            _uowFactory = uowFactory;
            _configRepository = configRepository;
        }

        public override async Task Initialize()
        {
            var configuration = DocuSignManager.SetUp(AuthorizationToken);
            ActivityUI.TemplateSelector.ListItems.AddRange(DocuSignManager.GetTemplatesList(configuration)
                                                                          .Select(x => new ListItem { Key = x.Key, Value = x.Value }));
            ActivityUI.NotifierSelector.ListItems.AddRange((await HubCommunicator.GetActivityTemplates(Tags.Notifier, true))
                .Select(x => new ListItem { Key = x.Label, Value = x.Id.ToString() }));
            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RuntimeCrateLabel, true)
                          .AddField(DelayTimeProperty)
                          .AddField(ActionBeingTrackedProperty);
        }

        public override async Task FollowUp()
        {
            if (!ActivityUI.BuildSolutionButton.Clicked)
            {
                return;
            }
            ActivityPayload.ChildrenActivities.Clear();
            //We need to keep the versions we know how to work with. If later these child activities will be upgraded we probably won't be able to configure them properly
            var activityTemplates = await HubCommunicator.GetActivityTemplates();
            var configureMonitorActivityTask = ConfigureMonitorActivity(activityTemplates);
            var configureSetDelayTask = ConfigureSetDelayActivity(activityTemplates);
            var configureQueryFr8Task = ConfigureQueryFr8Activity(activityTemplates);
            var configureTestDataTask = ConfigureFilterDataActivity(activityTemplates);
            await Task.WhenAll(configureMonitorActivityTask, configureSetDelayTask, configureQueryFr8Task, configureTestDataTask);
            //If solution was already built and  we should replace notifier action
            var previousNotifierId = NotifierActivityId;
            var previousNotifierTemplateId = NotifierActivityTemplateId;
            var isInitialBuild = previousNotifierId == Guid.Empty;
            if (isInitialBuild)
            {
                await ConfigureBuildMessageActivity(activityTemplates);
            }
            var currentNotifierTemplateId = NotifierActivityTemplateId = Guid.Parse(ActivityUI.NotifierSelector.Value);
            if (isInitialBuild || currentNotifierTemplateId != previousNotifierTemplateId)
            {
                //If it is not initial build we should remove existing notifier from plan
                var previousNotifierOrdering = 3;
                if (!isInitialBuild)
                {
                    var currentPlan = (await HubCommunicator.GetPlansByActivity(ActivityId.ToString())).Plan;
                    var startingSubPlan = currentPlan.SubPlans.First(x => x.SubPlanId == currentPlan.StartingSubPlanId);
                    var previousNotifier = startingSubPlan.Activities.FirstOrDefault(x => x.Id == previousNotifierId);
                    if (previousNotifier != null)
                    {
                        previousNotifierOrdering = previousNotifier.Ordering;
                        await HubCommunicator.DeleteActivity(previousNotifierId);
                    }
                }
                //And we should add new notifier anyway
                NotifierActivityId = await ConfigureNotifierActivity(activityTemplates, previousNotifierOrdering);
            }
            ;
            ActivityPayload.ChildrenActivities.Sort((x, y) => x.Ordering.CompareTo(y.Ordering));
            ActivityPayload.ChildrenActivities[0] = configureMonitorActivityTask.Result;
            ActivityPayload.ChildrenActivities[2] = configureQueryFr8Task.Result;
        }

        private async Task<Guid> ConfigureNotifierActivity(List<ActivityTemplateDTO> activityTemplates, int previousNotifierOrdering)
        {
            var template = activityTemplates.First(x => x.Id == NotifierActivityTemplateId);
            var activity = await HubCommunicator.AddAndConfigureChildActivity(ActivityPayload.RootPlanNodeId.Value, template, order: previousNotifierOrdering);
            if (activity.ActivityTemplate.Name == "SendEmailViaSendGrid" && activity.ActivityTemplate.Version == "1")
            {
                var configControls = ControlHelper.GetConfigurationControls(activity.CrateStorage);
                var emailBodyField = ControlHelper.GetControl<TextSource>(configControls, "EmailBody", ControlTypes.TextSource);
                emailBodyField.ValueSource = "upstream";
                emailBodyField.Value = NotificationMessageLabel;
                emailBodyField.selectedKey = NotificationMessageLabel;
                var emailSubjectField = ControlHelper.GetControl<TextSource>(configControls, "EmailSubject", ControlTypes.TextSource);
                emailSubjectField.ValueSource = "specific";
                emailSubjectField.TextValue = "Fr8 Notification Message";
            }
            else if (activity.ActivityTemplate.Name == "Send_Via_Twilio" && activity.ActivityTemplate.Version == "1")
            {
                var configControls = ControlHelper.GetConfigurationControls(activity.CrateStorage);
                var emailBodyField = ControlHelper.GetControl<TextSource>(configControls, "SMS_Body", ControlTypes.TextSource);
                emailBodyField.ValueSource = "upstream";
                emailBodyField.Value = NotificationMessageLabel;
                emailBodyField.selectedKey = NotificationMessageLabel;
            }
            else if (activity.ActivityTemplate.Name == "Publish_To_Slack" && activity.ActivityTemplate.Version == "2")
            {
                if (activity.CrateStorage.FirstCrateOrDefault<StandardAuthenticationCM>() == null)
                {
                    var configControls = ControlHelper.GetConfigurationControls(activity.CrateStorage);
                    var messageField = ControlHelper.GetControl<TextSource>(configControls, "MessageSource", ControlTypes.TextSource);
                    messageField.ValueSource = "upstream";
                    messageField.Value = NotificationMessageLabel;
                    messageField.selectedKey = NotificationMessageLabel;
                    messageField.SelectedItem = new FieldDTO { Name = NotificationMessageLabel };
                    activity = await HubCommunicator.ConfigureActivity(activity);
                }
            }
            return activity.Id;
        }

        private async Task ConfigureBuildMessageActivity(List<ActivityTemplateDTO> activityTemplates)
        {
            var template = activityTemplates.Single(x => x.Terminal.Name == "terminalFr8Core" && x.Name == "Build_Message" && x.Version == "1");
            var activity = await HubCommunicator.AddAndConfigureChildActivity(ActivityPayload.RootPlanNodeId.Value, template, order: 2);
            ControlHelper.SetControlValue(activity, "Body", MessageBody);
            ControlHelper.SetControlValue(activity, "Name", "NotificationMessage");
            await HubCommunicator.ConfigureActivity(activity);
        }

        private async Task ConfigureFilterDataActivity(List<ActivityTemplateDTO> activityTemplates)
        {
            var template = activityTemplates.Single(x => x.Terminal.Name == "terminalFr8Core" && x.Name == "Test_Incoming_Data" && x.Version == "1");
            var activity = await HubCommunicator.AddAndConfigureChildActivity(ActivityPayload, template, order: 4);
            var crateStorage = activity.CrateStorage;
            var configControlCM = crateStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .First();
            var filterPane = (FilterPane)configControlCM.Controls.First(x => x.Name == "Selected_Filter");
            var conditions = new List<FilterConditionDTO>
            {
                new FilterConditionDTO{ Field = "Status", Operator = "neq", Value = ActivityUI.RecipientEventSelector.Value }
            };
            filterPane.Value = JsonConvert.SerializeObject(new FilterDataDTO
            {
                ExecutionType = FilterExecutionType.WithFilter,
                Conditions = conditions
            });
            var queryableCriteria = new FieldDescriptionsCM(
                new FieldDTO
                {
                    Name = "Status",
                    Label = "Status",
                    FieldType = FieldType.String
                });
            var queryFieldsCrate = Crate.FromContent("Queryable Criteria", queryableCriteria);
            crateStorage.RemoveByLabel("Queryable Criteria");
            crateStorage.Add(queryFieldsCrate);
        }

        private async Task<ActivityPayload> ConfigureQueryFr8Activity(List<ActivityTemplateDTO> activityTemplates)
        {
            var template = activityTemplates.Single(x => x.Terminal.Name == "terminalFr8Core" && x.Name == "Query_Fr8_Warehouse" && x.Version == "1");
            var activity = await HubCommunicator.AddAndConfigureChildActivity(ActivityPayload, template, order: 3);
            var crateStorage = activity.CrateStorage;
            var configControlCM = ControlHelper.GetConfigurationControls(crateStorage);
            var radioButtonGroup = (RadioButtonGroup)configControlCM.Controls.First();
            radioButtonGroup.Radios[0].Selected = false;
            radioButtonGroup.Radios[1].Selected = true;
            var objectList = (DropDownList)radioButtonGroup.Radios[1].Controls.First(x => x.Name == "AvailableObjects");
            var selectedObject = GetMtType(ActivityUI.BasedOnTemplateOption.Selected ? typeof(DocuSignEnvelopeCM_v2) : typeof(DocuSignRecipientStatus));
            if (selectedObject == null)
            {
                return activity;
            }
            objectList.Value = selectedObject.Id.ToString("N");
            objectList.selectedKey = selectedObject.Alias;
            var filterPane = (FilterPane)radioButtonGroup.Radios[1].Controls.First(c => c.Name == "Filter");
            var conditions = new List<FilterConditionDTO>();
            if (ActivityUI.SentToSpecificRecipientOption.Selected)
            {
                conditions.Add(new FilterConditionDTO { Field = "Email", Operator = "eq", Value = ActivityUI.SpecificRecipientEmailText.Value });
            }
            else
            {
                conditions.Add(new FilterConditionDTO { Field = "EnvelopeId", Operator = "eq", Value = "FromPayload" });
            }
            filterPane.Value = JsonConvert.SerializeObject(new FilterDataDTO
            {
                ExecutionType = FilterExecutionType.WithFilter,
                Conditions = conditions
            });
            using (var uow = _uowFactory.Create())
            {
                var queryCriteria = Crate.FromContent("Queryable Criteria", new FieldDescriptionsCM(MTTypesHelper.GetFieldsByTypeId(uow, selectedObject.Id)));
                crateStorage.Add(queryCriteria);
            }
            return await HubCommunicator.ConfigureActivity(activity);
        }

        private MtTypeReference GetMtType(Type clrType)
        {
            using (var uow = _uowFactory.Create())
            {
                return uow.MultiTenantObjectRepository.FindTypeReference(clrType);
            }
        }

        private async Task ConfigureSetDelayActivity(List<ActivityTemplateDTO> activityTemplates)
        {
            var template = activityTemplates.Single(x => x.Terminal.Name == "terminalFr8Core" && x.Name == "Set_Delay" && x.Version == "1");
            var activity = await HubCommunicator.AddAndConfigureChildActivity(ActivityPayload, template, order: 2);
            ControlHelper.SetControlValue(activity, "Delay_Duration", ActivityUI.TimePeriod.Value);
        }

        private async Task<ActivityPayload> ConfigureMonitorActivity(List<ActivityTemplateDTO> activityTemplates)
        {
            var template = activityTemplates.Single(x => x.Terminal.Name == "terminalDocuSign" && x.Name == "Monitor_DocuSign_Envelope_Activity" && x.Version == "1");
            var activity = await HubCommunicator.AddAndConfigureChildActivity(ActivityPayload, template, order: 1);
            ControlHelper.SetControlValue(activity, "EnvelopeSent", "true");
            if (ActivityUI.SentToSpecificRecipientOption.Selected)
            {
                ControlHelper.SetControlValue(activity, "TemplateRecipientPicker.recipient.RecipientValue", ActivityUI.SpecificRecipientEmailText.Value);
            }
            else if (ActivityUI.BasedOnTemplateOption.Selected)
            {
                ControlHelper.SetControlValue(activity, "TemplateRecipientPicker.template.UpstreamCrate", ActivityUI.TemplateSelector.ListItems.Single(x => x.Key == ActivityUI.TemplateSelector.selectedKey));
            }
            return await HubCommunicator.ConfigureActivity(activity);
        }

        public override Task Run()
        {
            var resultFields = new List<KeyValueDTO>();
            var delayActivity = ActivityPayload.ChildrenActivities.FirstOrDefault(x => x.ActivityTemplate.Name == "Set_Delay" && x.ActivityTemplate.Version == "1");
            if (delayActivity != null)
            {
                var delayControl = delayActivity.CrateStorage.FirstCrate<StandardConfigurationControlsCM>().Content.Controls.OfType<Duration>().First();
                resultFields.Add(new KeyValueDTO { Key = DelayTimeProperty, Value = GetDelayDescription(delayControl) });
            }
            var filterActivity = ActivityPayload.ChildrenActivities.FirstOrDefault(x => x.ActivityTemplate.Name == "Test_Incoming_Data" && x.ActivityTemplate.Version == "1");
            if (filterActivity != null)
            {
                var filterPane = filterActivity.CrateStorage.FirstCrate<StandardConfigurationControlsCM>().Content.Controls.OfType<FilterPane>().First();
                var conditions = JsonConvert.DeserializeObject<FilterDataDTO>(filterPane.Value);
                var statusField = conditions.Conditions.FirstOrDefault(c => c.Field == "Status");
                if (statusField != null)
                {
                    resultFields.Add(new KeyValueDTO { Key = ActionBeingTrackedProperty, Value = statusField.Value });
                }
            }
            Payload.Add(Crate<StandardPayloadDataCM>.FromContent(RuntimeCrateLabel, new StandardPayloadDataCM(resultFields)));
            return Task.FromResult(0);
        }

        private string GetDelayDescription(Duration delayControl)
        {
            if (delayControl.Days == 0 && delayControl.Hours == 0 && delayControl.Minutes == 0)
            {
                return "none";
            }
            var result = new StringBuilder();
            if (delayControl.Days != 0)
            {
                result.Append($"{delayControl.Days} day{(delayControl.Days == 1 ? string.Empty : "s")}");
            }
            if (delayControl.Hours != 0)
            {
                if (result.Length > 0)
                {
                    result.Append(' ');
                }
                result.Append($"{delayControl.Hours} hour{(delayControl.Hours == 1 ? string.Empty : "s")}");
            }
            if (delayControl.Minutes != 0)
            {
                if (result.Length > 0)
                {
                    result.Append(' ');
                }
                result.Append($"{delayControl.Minutes} minute{(delayControl.Minutes == 1 ? string.Empty : "s")}");
            }
            return result.ToString();
        }

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        protected override Task Validate()
        {
            if (ActivityUI.BuildSolutionButton.Clicked)
            {
                if (ActivityUI.EnvelopeTypeSelectionGroup.Radios.All(x => !x.Selected))
                {
                    ValidationManager.SetError("Envelope option is not selected", ActivityUI.EnvelopeTypeSelectionGroup);
                }
                if (ActivityUI.SentToSpecificRecipientOption.Selected)
                {
                    ValidationManager.ValidateEmail(_configRepository, ActivityUI.SpecificRecipientEmailText);
                }
                if (ActivityUI.BasedOnTemplateOption.Selected)
                {
                    ValidationManager.ValidateTemplateList(ActivityUI.TemplateSelector);
                }
                if (string.IsNullOrEmpty(ActivityUI.RecipientEventSelector.Value))
                {
                    ValidationManager.SetError("Recipient action is not selected", ActivityUI.RecipientEventSelector);
                }
                if (string.IsNullOrEmpty(ActivityUI.NotifierSelector.Value))
                {
                    ValidationManager.SetError("Forward action is not selected", ActivityUI.NotifierSelector);
                }
                if (ValidationManager.HasErrors)
                {
                    ActivityUI.BuildSolutionButton.Clicked = false;
                }
            }
            return Task.FromResult(0);
        }

        private Guid NotifierActivityId
        {
            get
            {
                var result = this["NotifierId"];
                if (!string.IsNullOrEmpty(result))
                {
                    return Guid.Parse(result);
                }
                return Guid.Empty;
            }
            set { this["NotifierId"] = value.ToString(); }
        }

        private Guid NotifierActivityTemplateId
        {
            get
            {
                var result = this["NotifierTemplateId"];
                if (!string.IsNullOrEmpty(result))
                {
                    return Guid.Parse(result);
                }
                return Guid.Empty;
            }
            set { this["NotifierTemplateId"] = value.ToString(); }
        }
        /// <summary>
        /// This method provides documentation in two forms:
        /// SolutionPageDTO for general information and 
        /// ActivityResponseDTO for specific Help on minicon
        /// </summary>
        /// <param name="curDocumentation"></param>
        /// <returns></returns>
        protected override Task<DocumentationResponseDTO> GetDocumentation(string curDocumentation)
        {
            if (curDocumentation.Contains("MainPage"))
            {
                var curSolutionPage = new DocumentationResponseDTO(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);
            }
            if (curDocumentation.Contains("HelpMenu"))
            {
                if (curDocumentation.Contains("TrackDocuSignRecipients"))
                {
                    return Task.FromResult(new DocumentationResponseDTO(@"This solution work with notifications"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(new DocumentationResponseDTO(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(new DocumentationResponseDTO("Unknown contentPath"));
            }
            return
                Task.FromResult(
                    new DocumentationResponseDTO("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
        }
    }
}