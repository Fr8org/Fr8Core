using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces;
using Newtonsoft.Json;
using Data.Entities;
using Hub.Managers;
using StructureMap;
using TerminalBase.Infrastructure;
using TerminalBase.Services.MT;
using Data.States;
using Data.Repositories.MultiTenant;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using terminalDocuSign.Activities;

namespace terminalDocuSign.Actions
{
    public class Track_DocuSign_Recipients_v1 : BaseDocuSignActivity
    {
        private const string SolutionName = "Track DocuSign Recipients";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "DocuSign";
        private const string SolutionBody = @"<p>Link your important outgoing envelopes to Fr8's powerful notification activities, 
                                            which allow you to receive SMS notices, emails, or receive posts to popular tracking systems like Slack and Yammer. 
                                            Get notified when recipients take too long to sign!</p>";

        private const string MessageBody = @"Fr8 Alert: The DocuSign Envelope [TemplateName] has not been [ActionBeingTracked] by [RecipientUserName] at [RecipientEmail]. You had requested a notification after a delay of [DelayTime].";

        private class ActivityUi : StandardConfigurationControlsCM
        {
            public ActivityUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(new RadioButtonGroup()
                {
                    Name = "Track_Which_Envelopes",
                    Label = "Track which Envelopes?",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Radios = new List<RadioButtonOption>
                    {
                        new RadioButtonOption
                        {
                            Name = "SpecificRecipient",
                            Value = "Envelopes sent to a specific recipient",
                            Selected = true,
                            Controls = new List<ControlDefinitionDTO>
                            {
                                new TextBox()
                                {
                                    Label = "",
                                    Name = "SpecificRecipient"
                                }
                            }
                        },
                        new RadioButtonOption
                        {
                            Name = "SpecificTemplate",
                            Value = "Envelopes based on a specific template",
                            Controls = new List<ControlDefinitionDTO>
                            {
                                new DropDownList()
                                {
                                    Name = "SpecificTemplate",
                                    Source = new FieldSourceDTO
                                    {
                                        Label = "AvailableTemplates",
                                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                                }
                            }
                        }
                    }
                    }
                });
                Controls.Add(new Duration
                {
                    Label = "After you send a Tracked Envelope, Fr8 will wait.",
                    InnerLabel = "Wait this long:",
                    Name = "TimePeriod"
                });
                Controls.Add(new DropDownList
                {
                    Label = "Then Fr8 will notify you if a recipient has not",
                    Name = "RecipientEvent",
                    Source = new FieldSourceDTO
                    {
                        Label = "AvailableRecipientEvents",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    }
                });
                Controls.Add(new TextBlock
                {
                    Name = "EventInfo",
                    Label = "the Envelope."
                });

                Controls.Add(new DropDownList()
                {
                    Name = "NotificationHandler",
                    Label = "How would you like to be notified?",
                    Source = new FieldSourceDTO
                    {
                        Label = "AvailableHandlers",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    },
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                });
            }
        }
        

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(activityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(PackControls(new ActivityUi()));
                crateStorage.Add(PackAvailableTemplates(authTokenDO));
                crateStorage.Add(await PackAvailableHandlers(activityDO));
                crateStorage.Add(PackAvailableRecipientEvents(activityDO));
                crateStorage.Add(PackAvailableRunTimeDataFields());
            }

            return activityDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            var controls = CrateManager.GetStorage(activityDO)
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .First();

            var specificRecipientOption = ((RadioButtonGroup)controls.Controls[0]).Radios[0];
            var specificTemplateOption = ((RadioButtonGroup)controls.Controls[0]).Radios[1];
            var howToBeNotifiedDdl = (DropDownList)controls.FindByName("NotificationHandler");

            //let's don't add child actions to solution until how to be notified option is selected
            //FR-1873
            if (string.IsNullOrEmpty(howToBeNotifiedDdl.Value))
            {
                return activityDO;
            }
            var specificRecipient = specificRecipientOption.Controls.Single();
            if (specificRecipientOption.Selected && string.IsNullOrEmpty(specificRecipient.Value))
            {
                return activityDO;
            }

            var specificTemplate = specificTemplateOption.Controls.Single();
            if (specificTemplateOption.Selected && string.IsNullOrEmpty(specificTemplate.Value))
            {
                return activityDO;
            }

            var monitorDocusignAT = await GetActivityTemplate("terminalDocuSign", "Monitor_DocuSign_Envelope_Activity");
            var setDelayAT = await GetActivityTemplate("terminalFr8Core", "SetDelay");
            var queryFr8WareHouseAT = await GetActivityTemplate("terminalFr8Core", "QueryFr8Warehouse");
            var testIncomingDataAT = await GetActivityTemplate("terminalFr8Core", "TestIncomingData");
            var buildMessageAT = await GetActivityTemplate("terminalFr8Core", "Build_Message");

            //DocuSign
            var monitorDocuSignActionTask = AddAndConfigureChildActivity(activityDO, monitorDocusignAT, "Monitor Docusign Envelope Activity", "Monitor Docusign Envelope Activity", 1);
            var setDelayActionTask = AddAndConfigureChildActivity(activityDO, setDelayAT, "Set Delay", "Set Delay", 2);
            var queryFr8WarehouseActionTask = AddAndConfigureChildActivity(activityDO, queryFr8WareHouseAT, "Query Fr8 Warehouse", "Query Fr8 Warehouse", 3);
            var filterActionTask = AddAndConfigureChildActivity(activityDO, testIncomingDataAT, "Test Incoming Data", "Test Incoming Data", 4);

            var buildMessageActivityTask = AddAndConfigureChildActivity((Guid)activityDO.ParentPlanNodeId, buildMessageAT, "Build a Message", "Build a Message", 2);

            await Task.WhenAll(monitorDocuSignActionTask, setDelayActionTask, queryFr8WarehouseActionTask, filterActionTask, buildMessageActivityTask);

            var monitorDocuSignAction = monitorDocuSignActionTask.Result;
            var setDelayAction = setDelayActionTask.Result;
            var queryFr8WarehouseAction = queryFr8WarehouseActionTask.Result;
            var filterAction = filterActionTask.Result;
            // var notifierActivity = notifierActivityTask.Result;
            var buildMessageActivity = buildMessageActivityTask.Result;

            if (specificRecipientOption.Selected)
            {
                SetControlValue(monitorDocuSignAction, "TemplateRecipientPicker.recipient.RecipientValue", specificRecipientOption.Controls[0].Value);
            }
            else if (specificTemplateOption.Selected)
            {
                var ddlbTemplate = (specificTemplateOption.Controls[0] as DropDownList);
                SetControlValue(monitorDocuSignAction, "TemplateRecipientPicker.template.UpstreamCrate",
                   ddlbTemplate.ListItems.Single(a => a.Key == ddlbTemplate.selectedKey));
            }

            SetControlValue(buildMessageActivity, "Body", MessageBody);
            SetControlValue(buildMessageActivity, "Name", "NotificationMessage");

            buildMessageActivity = await HubCommunicator.ConfigureActivity(buildMessageActivity, CurrentFr8UserId);

            var notifierAT = await GetActivityTemplate(Guid.Parse(howToBeNotifiedDdl.Value));
            var notifierActivity = await AddAndConfigureChildActivity((Guid)activityDO.ParentPlanNodeId, notifierAT, howToBeNotifiedDdl.selectedKey, howToBeNotifiedDdl.selectedKey, 3);
            SetNotifierActivityBody(notifierActivity);

            SetControlValue(monitorDocuSignAction, "EnvelopeSent", "true");

            var configureNotifierTask = HubCommunicator.ConfigureActivity(notifierActivity, CurrentFr8UserId);

            //let's make followup configuration for monitorDocuSignEventAction
            //followup call places EventSubscription crate in storage
            var configureMonitorDocusignTask = HubCommunicator.ConfigureActivity(monitorDocuSignAction, CurrentFr8UserId);


            var durationControl = (Duration)controls.FindByName("TimePeriod");
            SetControlValue(setDelayAction, "Delay_Duration", durationControl.Value);
            await SetQueryFr8WarehouseActivityFields(queryFr8WarehouseAction, specificRecipientOption.Controls[0].Value);
            //let's make a followup configuration to fill criteria fields
            var configureQueryMTTask = HubCommunicator.ConfigureActivity(queryFr8WarehouseAction, CurrentFr8UserId);
            var recipientEventStatus = (DropDownList)controls.FindByName("RecipientEvent");
            SetFilterUsingRunTimeActivityFields(filterAction, recipientEventStatus.Value);

            await Task.WhenAll(configureMonitorDocusignTask, configureQueryMTTask, configureNotifierTask);

            monitorDocuSignAction = configureMonitorDocusignTask.Result;
            activityDO.ChildNodes = activityDO.ChildNodes.OrderBy(a => a.Ordering).ToList();
            activityDO.ChildNodes[0] = monitorDocuSignAction;

            return activityDO;
        }

        private void SetNotifierActivityBody(ActivityDO notifierActivity)
        {
            if (notifierActivity.ActivityTemplate.Name == "SendEmailViaSendGrid")
            {
                using (var updater = CrateManager.GetUpdatableStorage(notifierActivity))
                {
                    var configControls = GetConfigurationControls(updater);
                    var emailBodyField = (TextSource)GetControl(configControls, "EmailBody", ControlTypes.TextSource);
                    emailBodyField.ValueSource = "upstream";
                    emailBodyField.Value = "NotificationMessage";
                    emailBodyField.selectedKey = "NotificationMessage";

                    var emailSubjectField = (TextSource)GetControl(configControls, "EmailSubject", ControlTypes.TextSource);
                    emailSubjectField.ValueSource = "specific";
                    emailSubjectField.TextValue = "Fr8 Notification Message";
                }


            }
            else if (notifierActivity.ActivityTemplate.Name == "Send_Via_Twilio")
            {
                using (var updater = CrateManager.GetUpdatableStorage(notifierActivity))
                {
                    var configControls = GetConfigurationControls(updater);
                    var emailBodyField = (TextSource)GetControl(configControls, "SMS_Body", ControlTypes.TextSource);
                    emailBodyField.ValueSource = "upstream";
                    emailBodyField.Value = "NotificationMessage";
                    emailBodyField.selectedKey = "NotificationMessage";
                }
            }
            else if (notifierActivity.ActivityTemplate.Name == "Publish_To_Slack")
            {
                using (var updater = CrateManager.GetUpdatableStorage(notifierActivity))
                {
                    var configControls = GetConfigurationControls(updater);
                    if (configControls == null)
                    {
                        //user is not authenticated yet - there is nothing we can do now
                        return;
                    }
                    var messageField = (TextSource)GetControl(configControls, "Select_Message_Field", ControlTypes.TextSource);
                    if (messageField == null)
                    {
                        //user is not authenticated yet - there is nothing we can do now
                        return;
                    }
                    messageField.ValueSource = "upstream";
                    messageField.Value = "NotificationMessage";
                    messageField.selectedKey = "NotificationMessage";
                }
            }

        }

        private void SetFilterUsingRunTimeActivityFields(ActivityDO filterUsingRunTimeAction, string status)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(filterUsingRunTimeAction))
            {
                var configControlCM = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .First();

                var filterPane = (FilterPane)configControlCM.Controls.First(c => c.Name == "Selected_Filter");

                var conditions = new List<FilterConditionDTO>
                {
                    new FilterConditionDTO{ Field = "Status", Operator = "neq", Value = status}
                };

                filterPane.Value = JsonConvert.SerializeObject(new FilterDataDTO
                {
                    ExecutionType = FilterExecutionType.WithFilter,
                    Conditions = conditions
                });

                var queryableCriteria = new FieldDescriptionsCM(
                    new FieldDTO()
                    {
                        Key = "Status",
                        Label = "Status",
                        FieldType = FieldType.String
                    });
                var queryFieldsCrate = Fr8Data.Crates.Crate.FromContent("Queryable Criteria", queryableCriteria);
                crateStorage.RemoveByLabel("Queryable Criteria");
                crateStorage.Add(queryFieldsCrate);
            }
        }

        private async Task SetQueryFr8WarehouseActivityFields(ActivityDO queryFr8Warehouse, string recipientEmail)
        {
            //update action's duration value
            using (var crateStorage = CrateManager.GetUpdatableStorage(queryFr8Warehouse))
            {
                var configControlCM = GetConfigurationControls(crateStorage);
                var radioButtonGroup = (configControlCM.Controls.First() as RadioButtonGroup);
                radioButtonGroup.Radios[0].Selected = false;
                radioButtonGroup.Radios[1].Selected = true;
                var objectList = (DropDownList)(radioButtonGroup.Radios[1].Controls.FirstOrDefault(c => c.Name == "AvailableObjects"));
                MtTypeReference selectedObject;

                if (string.IsNullOrEmpty(recipientEmail))
                {
                    selectedObject = GetMtType(typeof(DocuSignEnvelopeCM_v2));
                }
                else
                {
                    selectedObject = GetMtType(typeof(DocuSignRecipientStatus));
                }

                if (selectedObject == null)
                {
                    return;
                }

                objectList.Value = selectedObject.Id.ToString("N");
                objectList.selectedKey = selectedObject.Alias;

                var filterPane = (FilterPane)radioButtonGroup.Radios[1].Controls.First(c => c.Name == "Filter");

                var conditions = new List<FilterConditionDTO>
                                {
                                    new FilterConditionDTO{ Field = "EnvelopeId", Operator = "eq", Value = "FromPayload"}
                                };

                if (recipientEmail != null)
                {
                    conditions.Add(new FilterConditionDTO { Field = "Email", Operator = "eq", Value = recipientEmail });
                }

                filterPane.Value = JsonConvert.SerializeObject(new FilterDataDTO
                {
                    ExecutionType = FilterExecutionType.WithFilter,
                    Conditions = conditions
                });

                var queryCriteria = Fr8Data.Crates.Crate.FromContent(
                    "Queryable Criteria",
                    new FieldDescriptionsCM(MTTypesHelper.GetFieldsByTypeId(selectedObject.Id))
                );
                crateStorage.Add(queryCriteria);
            }
        }

        private MtTypeReference GetMtType(Type clrType)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.MultiTenantObjectRepository.FindTypeReference(clrType);
            }
        }


        private Crate PackAvailableTemplates(AuthorizationTokenDO authTokenDO)
        {
            var conf = DocuSignManager.SetUp(authTokenDO);
            var fields = DocuSignManager.GetTemplatesList(conf);

            var crate = Crate.CreateDesignTimeFieldsCrate(
                "AvailableTemplates",
                AvailabilityType.Configuration,
                fields.ToArray());
            return crate;
        }

        private Crate PackAvailableRecipientEvents(ActivityDO activityDO)
        {
            var events = new[] { "Taken Delivery", "Signed" };

            var availableRecipientEventsCrate =
                CrateManager.CreateDesignTimeFieldsCrate(
                    "AvailableRecipientEvents", events.Select(x => new FieldDTO(x, x)).ToArray()
                );

            return availableRecipientEventsCrate;
        }

        private Crate PackAvailableRunTimeDataFields()
        {
            var events = new[] { "ActionBeingTracked", "DelayTime" };

            var availableRecipientEventsCrate =
                CrateManager.CreateDesignTimeFieldsCrate(
                    "AvailableRunTimeDataFields", events.Select(x => new FieldDTO(x, x)).ToArray()
                );

            return availableRecipientEventsCrate;
        }

        private async Task<Crate> PackAvailableHandlers(ActivityDO activityDO)
        {
            var templates = await HubCommunicator.GetActivityTemplates(CurrentFr8UserId);
            var taggedTemplates = templates.Where(x => x.Tags != null && x.Tags.Contains("Notifier"));

            var availableHandlersCrate =
                CrateManager.CreateDesignTimeFieldsCrate(
                    "AvailableHandlers",
                    taggedTemplates.Select(x => new FieldDTO(x.Label, x.Id.ToString())).ToArray()
                );

            return availableHandlersCrate;
        }

        protected override string ActivityUserFriendlyName => SolutionName;

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);

            var configControls = (await HubCommunicator.GetCratesByDirection<StandardConfigurationControlsCM>(curActivityDO, CrateDirection.Downstream, CurrentFr8UserId)).SelectMany(c => c.Content.Controls);

            var delayValue = (Duration)configControls.Single(c => c.Name == "Delay_Duration" && c.Type == ControlTypes.Duration);

            var runTimePayloadData = new List<FieldDTO>();
            var delayTimeString = delayValue.Days + " days, " + delayValue.Hours + " hours and " + delayValue.Minutes + " minutes";
            runTimePayloadData.Add(new FieldDTO("DelayTime", delayTimeString, AvailabilityType.RunTime));

            var filterPane = (FilterPane)configControls.Single(c => c.Name == "Selected_Filter" && c.Type == ControlTypes.FilterPane);

            var conditions = JsonConvert.DeserializeObject<FilterDataDTO>(filterPane.Value);

            var statusField = conditions.Conditions.FirstOrDefault(c => c.Field == "Status");
            if (statusField != null)
            {
                runTimePayloadData.Add(new FieldDTO("ActionBeingTracked", statusField.Value, AvailabilityType.RunTime));
            }

            using (var crateStorage = CrateManager.GetUpdatableStorage(payload))
            {
                crateStorage.Add(Fr8Data.Crates.Crate.FromContent("Track DocuSign Recipients Payload Data", new StandardPayloadDataCM(runTimePayloadData)));
            }


            return Success(payload);
        }
        /// <summary>
        /// This method provides documentation in two forms:
        /// SolutionPageDTO for general information and 
        /// ActivityResponseDTO for specific Help on minicon
        /// </summary>
        /// <param name="activityDO"></param>
        /// <param name="curDocumentation"></param>
        /// <returns></returns>
        public dynamic Documentation(ActivityDO activityDO, string curDocumentation)
        {
            if (curDocumentation.Contains("MainPage"))
            {
                var curSolutionPage = GetDefaultDocumentation(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);
            }
            if (curDocumentation.Contains("HelpMenu"))
            {
                if (curDocumentation.Contains("TrackDocuSignRecipients"))
                {
                    return Task.FromResult(GenerateDocumentationResponse(@"This solution work with notifications"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(GenerateDocumentationResponse(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(GenerateErrorResponse("Unknown contentPath"));
            }
            return
                Task.FromResult(
                    GenerateErrorResponse("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
        }
    }
}