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
    public class Rich_Document_Notifications_v1 : BaseDocuSignAction
    {
        private const string SolutionName = "Rich Document Notifications";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "DocuSign";
        private class ActionUi : StandardConfigurationControlsCM
        {
            public ActionUi()
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
                /*
                Controls.Add(new DropDownList()
                {
                    Name = "SpecificEvent",
                    Label = "What event do you want to watch for?",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Source = new FieldSourceDTO
                    {
                        Label = "AvailableEvents",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    }
                });
                */

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
            if (Crate.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(activityDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.Add(PackAvailableTemplates(authTokenDO));
                updater.CrateStorage.Add(PackAvailableEvents());
                updater.CrateStorage.Add(await PackAvailableHandlers(activityDO));
                updater.CrateStorage.Add(PackAvailableRecipientEvents(activityDO));
            }

            return activityDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            var controls = Crate.GetStorage(activityDO)
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
            var notifierActivityTask = AddAndConfigureChildActivity(activityDO, howToBeNotifiedDdl.Value, howToBeNotifiedDdl.selectedKey, howToBeNotifiedDdl.selectedKey, 5);

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
                   ddlbTemplate.ListItems.Where(a => a.Key == ddlbTemplate.selectedKey).Single());
            }

            SetControlValue(monitorDocuSignAction, "Event_Envelope_Sent", "true");

            //let's make followup configuration for monitorDocuSignEventAction
            //followup call places EventSubscription crate in storage
            var configureMonitorDocusignTask = HubCommunicator.ConfigureActivity(monitorDocuSignAction, CurrentFr8UserId);
            

            var durationControl = (Duration)controls.FindByName("TimePeriod");
            SetControlValue(setDelayAction, "Delay_Duration", durationControl.Value);
            await SetQueryFr8WarehouseActionFields(queryFr8WarehouseAction, specificRecipientOption.Controls[0].Value);
            //let's make a followup configuration to fill criteria fields
            var configureQueryMTTask = HubCommunicator.ConfigureActivity(queryFr8WarehouseAction, CurrentFr8UserId);
            var recipientEventStatus = (DropDownList)controls.FindByName("RecipientEvent");
            SetFilterUsingRunTimeActionFields(filterAction, recipientEventStatus.Value);

            await Task.WhenAll(configureMonitorDocusignTask, configureQueryMTTask);

            monitorDocuSignAction = configureMonitorDocusignTask.Result;
            activityDO.ChildNodes = activityDO.ChildNodes.OrderBy(a => a.Ordering).ToList();
            activityDO.ChildNodes[0] = monitorDocuSignAction;

            return activityDO;
        }

        private void SetFilterUsingRunTimeActionFields(ActivityDO filterUsingRunTimeAction, string status)
        {
            using (var updater = Crate.UpdateStorage(filterUsingRunTimeAction))
            {
                var configControlCM = updater.CrateStorage
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

                var queryFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Queryable Criteria", new FieldDTO[] { new FieldDTO("Status", "Status") });
                updater.CrateStorage.RemoveByLabel("Queryable Criteria");
                updater.CrateStorage.Add(queryFieldsCrate);
            }
        }

        private async Task SetQueryFr8WarehouseActionFields(ActivityDO queryFr8Warehouse, string recipientEmail)
        {
            //update action's duration value
            using (var updater = Crate.UpdateStorage(queryFr8Warehouse))
            {
                var configControlCM = GetConfigurationControls(updater.CrateStorage);
                var radioButtonGroup = (configControlCM.Controls.First() as RadioButtonGroup);
                radioButtonGroup.Radios[0].Selected = false;
                radioButtonGroup.Radios[1].Selected = true;
                var objectList = (DropDownList)(radioButtonGroup.Radios[1].Controls.Where(c => c.Name == "AvailableObjects").FirstOrDefault());
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

                updater.CrateStorage.Add(Crate.CreateDesignTimeFieldsCrate("Queryable Criteria", GetFieldsByObjectId(selectedObject.Id).ToArray()));
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

        private Crate PackAvailableEvents()
        {
            var crate = Crate.CreateDesignTimeFieldsCrate(
                "AvailableEvents",
                new FieldDTO { Key = "You sent a Docusign Envelope", Value = "Event_Envelope_Sent" },
                new FieldDTO { Key = "Someone received an Envelope you sent", Value = "Event_Envelope_Received" },
                new FieldDTO { Key = "One of your Recipients signed an Envelope", Value = "Event_Recipient_Signed" }
                //,new FieldDTO { Key = "Recipient Sent", Value = "Event_Recipient_Sent" }
            );

            return crate;
        }

        private Crate PackAvailableRecipientEvents(ActivityDO activityDO)
        {
            var events = new[] { "Delivered", "Signed", "Declined", "AutoResponded" };

            var availableRecipientEventsCrate =
                Crate.CreateDesignTimeFieldsCrate(
                    "AvailableRecipientEvents", events.Select(x => new FieldDTO(x, x)).ToArray()
                );

            return availableRecipientEventsCrate;
        }

        private async Task<Crate> PackAvailableHandlers(ActivityDO activityDO)
        {
            var templates = await HubCommunicator.GetActivityTemplates(activityDO, CurrentFr8UserId);
            var taggedTemplates = templates.Where(x => x.Tags != null && x.Tags.Contains("Notifier"));

            var availableHandlersCrate =
                Crate.CreateDesignTimeFieldsCrate(
                    "AvailableHandlers",
                    taggedTemplates.Select(x => new FieldDTO(x.Label, x.Id.ToString())).ToArray()
                );

            return availableHandlersCrate;
        }



        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActivityDO, containerId));
        }
        //This method provides some documentation for the DocuSign Solution Actions
        public Task<SolutionPageDTO> Documentation(ActivityDO activityDO)
        {
            var curSolutionPage = new SolutionPageDTO
            {
                Name = SolutionName,
                Version = SolutionVersion,
                Terminal = TerminalName,
                Body = @"<p>This is a solution action</p>"
            };
            return Task.FromResult(curSolutionPage);
        }
    }
}