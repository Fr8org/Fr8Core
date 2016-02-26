using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Interfaces;
using Newtonsoft.Json;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using StructureMap;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalDocuSign.Infrastructure;
using AutoMapper;

namespace terminalDocuSign.Actions
{
    public class Rich_Document_Notifications_v1 : BaseDocuSignActivity
    {
        private const string SolutionName = "Rich Document Notifications";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "DocuSign";
        private const string SolutionBody = @"<p>Link your important outgoing envelopes to Fr8's powerful notification Activities, 
                                            which allow you to receive SMS notices, emails, or receive posts to popular tracking systems like Slack and Yammer. 
                                            Get notified when recipients take too long to sign!</p>";

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
                    InnerLabel = "Wait how long:",
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
                    Label = "the Envelope"
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

        public DocuSignManager DocuSignManager { get; set; }

        public Rich_Document_Notifications_v1()
        {
            DocuSignManager = new DocuSignManager();
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
            
            //DocuSign
            var monitorDocuSignActionTask = AddAndConfigureChildActivity(activityDO, "Monitor_DocuSign_Envelope_Activity", "Monitor Docusign Envelope Activity", "Monitor Docusign Envelope Activity", 1);
            var setDelayActionTask = AddAndConfigureChildActivity(activityDO, "SetDelay", "Set Delay", "Set Delay", 2);
            var queryFr8WarehouseActionTask = AddAndConfigureChildActivity(activityDO, "QueryFr8Warehouse", "Query Fr8 Warehouse", "Query Fr8 Warehouse", 3);
            var filterActionTask = AddAndConfigureChildActivity(activityDO, "TestIncomingData", "Test Incoming Data", "Test Incoming Data", 4);
            var notifierActivityTask = AddAndConfigureChildActivity((Guid)activityDO.ParentRouteNodeId, howToBeNotifiedDdl.Value, howToBeNotifiedDdl.selectedKey, howToBeNotifiedDdl.selectedKey, 2);

            await Task.WhenAll(monitorDocuSignActionTask, setDelayActionTask, queryFr8WarehouseActionTask, filterActionTask, notifierActivityTask);

            var monitorDocuSignAction = monitorDocuSignActionTask.Result;
            var setDelayAction = setDelayActionTask.Result;
            var queryFr8WarehouseAction = queryFr8WarehouseActionTask.Result;
            var filterAction = filterActionTask.Result;
            var notifierActivity = notifierActivityTask.Result;

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

            SetControlValue(monitorDocuSignAction, "Event_Envelope_Sent", "true");

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

            await Task.WhenAll(configureMonitorDocusignTask, configureQueryMTTask);

            monitorDocuSignAction = configureMonitorDocusignTask.Result;
            activityDO.ChildNodes = activityDO.ChildNodes.OrderBy(a => a.Ordering).ToList();
            activityDO.ChildNodes[0] = monitorDocuSignAction;

            return activityDO;
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

                var queryFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Queryable Criteria", new FieldDTO[] { new FieldDTO("Status", "Status") });
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
                MT_Object selectedObject;
                if (string.IsNullOrEmpty(recipientEmail))
                {
                    selectedObject = await GetMTObject(MT.DocuSignEvent);
                }
                else
                {
                    selectedObject = await GetMTObject(MT.DocuSignRecipient);
                }

                if (selectedObject == null)
                {
                    return;
                }

                objectList.Value = selectedObject.Id.ToString(CultureInfo.InvariantCulture);
                objectList.selectedKey = selectedObject.Name;

                var filterPane = (FilterPane)radioButtonGroup.Radios[1].Controls.First(c => c.Name == "Filter");

                var conditions = new List<FilterConditionDTO>
                                {
                                    new FilterConditionDTO{ Field = "EnvelopeId", Operator = "eq", Value = "FromPayload"}
                                };

                if (recipientEmail != null)
                {
                    conditions.Add(new FilterConditionDTO { Field = "RecipientEmail", Operator = "eq", Value = recipientEmail });
                }

                filterPane.Value = JsonConvert.SerializeObject(new FilterDataDTO
                {
                    ExecutionType = FilterExecutionType.WithFilter,
                    Conditions = conditions
                });

                crateStorage.Add(CrateManager.CreateDesignTimeFieldsCrate("Queryable Criteria", GetFieldsByObjectId(selectedObject.Id).ToArray()));
            }
        }

        private IEnumerable<FieldDTO> GetFieldsByObjectId(int objectId)
        {
            var fields = new Dictionary<string, string>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var field in uow.MTFieldRepository.GetQuery().Where(x => x.MT_ObjectId == objectId))
                {
                    var alias = "Value" + field.FieldColumnOffset;
                    string existingAlias;

                    if (fields.TryGetValue(field.Name, out existingAlias))
                    {
                        if (existingAlias != alias)
                        {
                            throw new InvalidOperationException(string.Format("Duplicate field definition. MT object type: {0}. Field {1} is mapped to {2} and {3}", objectId, field.Name, existingAlias, alias));
                        }
                    }
                    else
                    {
                        fields[field.Name] = alias;
                    }
                }
            }

            return fields.OrderBy(x => x.Key).Select(x => new FieldDTO(x.Key, x.Key));
        }


        private async Task<MT_Object> GetMTObject(MT manifestType)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return await uow.MTObjectRepository.GetQuery().FirstOrDefaultAsync(o => o.ManifestId == (int)MT.DocuSignRecipient);
            }
        }


        private Crate PackAvailableTemplates(AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert
                .DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

            var crate = DocuSignManager.PackCrate_DocuSignTemplateNames(docuSignAuthDTO);
            crate.Label = "AvailableTemplates";

            return crate;
        }

        private Crate PackAvailableRecipientEvents(ActivityDO activityDO)
        {
            var events = new[] { "Delivered", "Signed", "Declined", "AutoResponded" };

            var availableRecipientEventsCrate =
                CrateManager.CreateDesignTimeFieldsCrate(
                    "AvailableRecipientEvents", events.Select(x => new FieldDTO(x, x)).ToArray()
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



        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActivityDO, containerId));
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
                if (curDocumentation.Contains("ExplainRichDocumentation"))
                {
                    return Task.FromResult(GenerateDocumentationRepsonce(@"This solution work with notifications"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(GenerateDocumentationRepsonce(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(GenerateErrorRepsonce("Unknown contentPath"));
            }
            return
                Task.FromResult(
                    GenerateErrorRepsonce("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
        }
    }
}