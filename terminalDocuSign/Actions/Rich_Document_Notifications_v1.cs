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
                    Label = "After you send a Tracked Envelope, Fr8 will wait this long",
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


        public ExplicitConfigurationHelper ExplicitConfigurationHelper { get; set; }
        public DocuSignManager DocuSignManager { get; set; }

        public Rich_Document_Notifications_v1()
        {
            DocuSignManager = new DocuSignManager();
            ExplicitConfigurationHelper = new ExplicitConfigurationHelper();
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
                updater.CrateStorage.Add(await PackAvailableRecipientEvents(activityDO));
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
            var howToBeNotifiedDdl = (DropDownList) controls.FindByName("NotificationHandler");

            //let's don't add child actions to solution until how to be notified option is selected
            //FR-1873
            if (string.IsNullOrEmpty(howToBeNotifiedDdl.Value))
            {
                return activityDO;
            }

            activityDO.ChildNodes = new List<RouteNodeDO>();

            int ordering = 0;

            var activityList = await HubCommunicator.GetActivityTemplates(activityDO, CurrentFr8UserId);

            var monitorDocuSignTemplate = GetActivityTemplate(activityList, "Monitor_DocuSign_Envelope_Activity");
            var monitorDocuSignAction = await CreateMonitorDocuSignAction(monitorDocuSignTemplate, authTokenDO, activityDO);

            

            string recipientEmail = null;
            if (specificRecipientOption.Selected)
            {
                recipientEmail = SetMonitorDocuSignSpecificRecipient(monitorDocuSignAction, specificRecipientOption);
            }
            else if (specificTemplateOption.Selected)
            {
                SetMonitorDocuSignSpecificTemplate(monitorDocuSignAction, specificTemplateOption);
            }

            SetMonitorDocuSignSpecificEvent(monitorDocuSignAction);

            //let's make followup configuration for monitorDocuSignEventAction
            //followup call places EventSubscription crate in storage
            monitorDocuSignAction = await HubCommunicator.ConfigureActivity(monitorDocuSignAction, CurrentFr8UserId);
            monitorDocuSignAction.AuthorizationToken = authTokenDO;
            monitorDocuSignAction.AuthorizationTokenId = authTokenDO.Id;
            activityDO.ChildNodes.Add(monitorDocuSignAction);

            
            var setDelayTemplate = GetActivityTemplate(activityList, "SetDelay");
            var durationControl = (Duration)controls.FindByName("TimePeriod");
            var setDelayAction = await CreateSetDelayAction(setDelayTemplate, activityDO);
            SetDelayActionFields(setDelayAction, durationControl);
            activityDO.ChildNodes.Add(setDelayAction);

            var queryMTDatabaseTemplate = GetActivityTemplate(activityList, "QueryMTDatabase");
            var queryMTDatabaseAction = await CreateQueryMTDatabaseAction(queryMTDatabaseTemplate, activityDO);
            await SetQueryMTDatabaseActionFields(queryMTDatabaseAction, recipientEmail);
            //let's make a followup configuration to fill criteria fields
                
            queryMTDatabaseAction = await HubCommunicator.ConfigureActivity(queryMTDatabaseAction, CurrentFr8UserId);
            activityDO.ChildNodes.Add(queryMTDatabaseAction);

            var recipientEventStatus = (DropDownList)controls.FindByName("RecipientEvent");


            var filterUsingRuntimeTemplate = GetActivityTemplate(activityList, "TestIncomingData");
            var filterAction = await CreateFilterUsingRunTimeAction(filterUsingRuntimeTemplate, activityDO);
            SetFilterUsingRunTimeActionFields(filterAction, recipientEventStatus.Value);
            activityDO.ChildNodes.Add(filterAction);
            


            var notifierAction = CreateNotifierAction(activityList, activityDO, howToBeNotifiedDdl, ++ordering);
            if (notifierAction != null)
            {
                activityDO.ChildNodes.Add(notifierAction);
            }

            return activityDO;
        }

        private ActivityTemplateDTO GetActivityTemplate(IEnumerable<ActivityTemplateDTO> activityList, string activityTemplateName)
        {
            var template = activityList.FirstOrDefault(x => x.Name == activityTemplateName);
            if (template == null)
            {
                throw new Exception(string.Format("ActivityTemplate {0} was not found", activityTemplateName));
            }

            return template;
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

        private async Task SetQueryMTDatabaseActionFields(ActivityDO queryMTDatabase, string recipientEmail)
        {
            //update action's duration value
            using (var updater = Crate.UpdateStorage(queryMTDatabase))
            {
                var configControlCM = updater.CrateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .First();

                var objectList = (DropDownList)configControlCM.Controls.First(c => c.Name == "AvailableObjects");
                MT_Object selectedObject;
                if (recipientEmail == null)
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

                var filterPane = (FilterPane)configControlCM.Controls.First(c => c.Name == "Filter");

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

                
            }
        }

        private void SetDelayActionFields(ActivityDO setDelayAction, Duration externalDurationControl)
        {
            //update action's duration value
            using (var updater = Crate.UpdateStorage(setDelayAction))
            {
                var configControlCM = updater.CrateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .First();

                var duration = (Duration)configControlCM.Controls.First();
                duration.Days = externalDurationControl.Days;
                duration.Hours = externalDurationControl.Hours;
                duration.Minutes = externalDurationControl.Minutes;
            }
        }

        #region Action_Creation

        private async Task<ActivityDO> CreateFilterUsingRunTimeAction(ActivityTemplateDTO template, ActivityDO parentAction)
        {
            var activity = await HubCommunicator.CreateAndConfigureActivity(template.Id, "Filter Using Run Time", CurrentFr8UserId, "Filter Using Run Time", 4, parentAction.Id, false);
            return Mapper.Map<ActivityDO>(activity);
        }
        private async Task<ActivityDO> CreateQueryMTDatabaseAction(ActivityTemplateDTO template, ActivityDO parentAction)
        {
            var activity = await HubCommunicator.CreateAndConfigureActivity(template.Id, "Query MT Database", CurrentFr8UserId, "Query MT Database", 3, parentAction.Id, false);
            return Mapper.Map<ActivityDO>(activity);
        }

        private async Task<ActivityDO> CreateSetDelayAction(ActivityTemplateDTO template, ActivityDO parentAction)
        {
            var activity = await HubCommunicator.CreateAndConfigureActivity(template.Id, "Set Delay", CurrentFr8UserId, "Set Delay", 2, parentAction.Id, false);
            return Mapper.Map<ActivityDO>(activity);
        }

        private async Task<ActivityDO> CreateMonitorDocuSignAction(ActivityTemplateDTO template, AuthorizationTokenDO authTokenDO, ActivityDO parentAction)
        {
            var activity = await HubCommunicator.CreateAndConfigureActivity(template.Id, "Monitor Docusign", CurrentFr8UserId, "Monitor DocuSign", 1, parentAction.Id, false, authTokenDO.Id);
            return Mapper.Map<ActivityDO>(activity);
        }

        #endregion

        #region Monitor_DocuSign routines.
        private string SetMonitorDocuSignSpecificRecipient(ActivityDO monitorAction, RadioButtonOption source)
        {
            using (var updater = Crate.UpdateStorage(monitorAction))
            {
                var controls = updater.CrateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .First();

                var recipientOption = ((RadioButtonGroup)controls.Controls.Last()).Radios[0];
                recipientOption.Selected = true;
                ((TextBox)recipientOption.Controls[0]).Value = ((TextBox)source.Controls[0]).Value;

                var templateOption = ((RadioButtonGroup)controls.Controls.Last()).Radios[1];
                templateOption.Selected = false;
                return ((TextBox)recipientOption.Controls[0]).Value;
            }
        }

        private void SetMonitorDocuSignSpecificTemplate(ActivityDO monitorAction, RadioButtonOption option)
        {
            using (var updater = Crate.UpdateStorage(monitorAction))
            {
                var controls = updater.CrateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .First();

                var recipientOption = ((RadioButtonGroup)controls.Controls.Last()).Radios[0];
                recipientOption.Selected = false;

                var templateOption = ((RadioButtonGroup)controls.Controls.Last()).Radios[1];
                templateOption.Selected = true;
                ((DropDownList)templateOption.Controls[0]).selectedKey = ((DropDownList)option.Controls[0]).selectedKey;
                ((DropDownList)templateOption.Controls[0]).Value = ((DropDownList)option.Controls[0]).Value;
            }
        }

        private void SetMonitorDocuSignSpecificEvent(ActivityDO monitorAction)
        {
            using (var updater = Crate.UpdateStorage(monitorAction))
            {
                var controls = updater.CrateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .First();

                
                var checkBoxes = controls.Controls
                    .Where(x => x.Type == ControlTypes.CheckBox)
                    .ToList();

                checkBoxes.ForEach(x => { x.Selected = false; });

                var selectedCheckBox = checkBoxes.FirstOrDefault(x => x.Name == "Event_Envelope_Sent");
                if (selectedCheckBox != null)
                {
                    selectedCheckBox.Selected = true;
        }

            }
        }
        /*
        private void SetMonitorDocuSignSpecificEvent(ActionDO monitorAction, DropDownList ddl)
        {
            using (var updater = Crate.UpdateStorage(monitorAction))
            {
                var controls = updater.CrateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .First();

                if (ddl.ListItems != null)
                {
                    var checkBoxes = controls.Controls
                        .Where(x => x.Type == ControlTypes.CheckBox
                            && ddl.ListItems.Any(y => y.Value == x.Name)
                        )
                        .ToList();

                    checkBoxes.ForEach(x => { x.Selected = false; });

                    var selectedCheckBox = checkBoxes.FirstOrDefault(x => x.Name == ddl.Value);
                    if (selectedCheckBox != null)
                    {
                        selectedCheckBox.Selected = true;
                    }
                }
            }
        }*/

        private ActivityDO CreateNotifierAction(IEnumerable<ActivityTemplateDTO> activityList, ActivityDO solutionAction, DropDownList ddl, int ordering)
        {
            
            var notifierAction = (ActivityDO)solutionAction.ChildNodes.FirstOrDefault(c => ((ActivityDO)c).ActivityTemplate.Tags != null && ((ActivityDO)c).ActivityTemplate.Tags.Contains("Notifier"));
            // Remove action if action types do not match.
            if (notifierAction != null)
            {
                //var handlerAction = (ActionDO)solutionAction.ChildNodes[1];
                var handlerActivityTemplateIdStr = notifierAction.ActivityTemplateId.HasValue
                    ? notifierAction.ActivityTemplateId.Value.ToString()
                    : string.Empty;

                if (handlerActivityTemplateIdStr != ddl.Value)
                {
                    solutionAction.ChildNodes.Remove(notifierAction);
                }
            }

            // Add action if no notifier action exist.
            if (!string.IsNullOrEmpty(ddl.Value) && notifierAction == null)
            {
                var selectedTemplate = activityList.FirstOrDefault(x => x.Id.ToString() == ddl.Value);

                if (selectedTemplate != null)
                {
                    var handlerAction = new ActivityDO
                    {
                        IsTempId = true,
                        ActivityTemplateId = selectedTemplate.Id,
                        CrateStorage = Crate.EmptyStorageAsStr(),
                        CreateDate = DateTime.Now,
                        Ordering = ordering,
                        Name = selectedTemplate.Label,
                        Label = selectedTemplate.Label
                    };
                    return handlerAction;
                }
            }

            return null;
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

        private async Task<Crate> PackAvailableRecipientEvents(ActivityDO activityDO)
        {
            var events = new []{"Created", "Sent", "Delivered", "Signed", "Declined", "FaxPending", "AutoResponded"};

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

        #endregion Monitor_DocuSign routines.

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActivityDO, containerId));
        }
    }
}