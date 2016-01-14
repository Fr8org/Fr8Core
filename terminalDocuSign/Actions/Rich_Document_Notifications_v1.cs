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

namespace terminalDocuSign.Actions
{
    public class Rich_Document_Notifications_v1 : BaseTerminalAction
    {
        private class ActionUi : StandardConfigurationControlsCM
        {
            public ActionUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(new RadioButtonGroup()
                {
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
                
                Controls.Add(new RadioButtonGroup()
                {
                    Name = "WhenToBeNotified",
                    Label = "When do you want to be notified?",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Radios = new List<RadioButtonOption>
                    {
                        new RadioButtonOption
                        {
                            Name = "NotificationMode",
                            Value = "When the event happens",
                            Selected = true
                        },
                        new RadioButtonOption
                        {
                            Name = "NotificationMode",
                            Value = "If a recipient hasn't taken an action within this amount of time",
                            Controls = new ControlDefinitionDTO[]
                            {
                                new Duration
                                {
                                    Label = "After you send a Tracked Envelope, Fr8 will wait this long",
                                    Name = "TimePeriod"
                                },
                                new DropDownList
                                {
                                    Label = "Then Fr8 will notify you if a recipient has not",
                                    Name = "RecipientEvent",
                                    Source = new FieldSourceDTO
                                    {
                                        Label = "AvailableRecipientEvents",
                                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                                    }
                                },
                                new TextBlock
                                {
                                    Name = "EventInfo",
                                    Label = "the Envelope"
                                }
                            }
                        }
                    }
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

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(actionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.Add(PackAvailableTemplates(authTokenDO));
                updater.CrateStorage.Add(PackAvailableEvents());
                updater.CrateStorage.Add(await PackAvailableHandlers(actionDO));
                updater.CrateStorage.Add(await PackAvailableRecipientEvents(actionDO));
            }

            return actionDO;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            var controls = Crate.GetStorage(actionDO)
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .First();

            var specificRecipientOption = ((RadioButtonGroup)controls.Controls[0]).Radios[0];
            var specificTemplateOption = ((RadioButtonGroup)controls.Controls[0]).Radios[1];
            var specificEventDdl = (DropDownList)controls.Controls[1];
            
            var whenToBeNotifiedRadioGrp = (RadioButtonGroup)controls.FindByName("WhenToBeNotified");
            var notifyWhenEventHappensRadio = whenToBeNotifiedRadioGrp.Radios[0];
            var notifyWhenEventDoesntHappenRadio = whenToBeNotifiedRadioGrp.Radios[1];
            var howToBeNotifiedDdl = (DropDownList) controls.FindByName("NotificationHandler");

            //let's don't add child actions to solution until how to be notified option is selected
            //FR-1873
            if (string.IsNullOrEmpty(howToBeNotifiedDdl.Value))
            {
                return actionDO;
            }

            actionDO.ChildNodes = new List<RouteNodeDO>();

            int ordering = 0;

            var activityList = await HubCommunicator.GetActivityTemplates(actionDO);

            var monitorDocuSignTemplate = GetActivityTemplate(activityList, "Monitor_DocuSign_Envelope_Activity");
            var monitorDocuSignAction = await CreateMonitorDocuSignAction(monitorDocuSignTemplate, authTokenDO, ++ordering);

            if (specificRecipientOption.Selected)
            {
                SetMonitorDocuSignSpecificRecipient(monitorDocuSignAction, specificRecipientOption);
            }
            else if (specificTemplateOption.Selected)
            {
                SetMonitorDocuSignSpecificTemplate(monitorDocuSignAction, specificTemplateOption);
            }

            SetMonitorDocuSignSpecificEvent(monitorDocuSignAction, specificEventDdl);

            //let's make followup configuration for monitorDocuSignEventAction
            //followup call places EventSubscription crate in storage
            monitorDocuSignAction = await ConfigureAction(monitorDocuSignTemplate, monitorDocuSignAction, authTokenDO);
            monitorDocuSignAction.AuthorizationToken = authTokenDO;
            monitorDocuSignAction.AuthorizationTokenId = authTokenDO.Id;
            actionDO.ChildNodes.Add(monitorDocuSignAction);

            if (notifyWhenEventDoesntHappenRadio.Selected)
            {
                var setDelayTemplate = GetActivityTemplate(activityList, "SetDelay");
                var durationControl = (Duration)notifyWhenEventDoesntHappenRadio.Controls.First(c => c.Name == "TimePeriod");
                var setDelayAction = await CreateSetDelayAction(setDelayTemplate, ++ordering);
                SetDelayActionFields(setDelayAction, durationControl);
                actionDO.ChildNodes.Add(setDelayAction);

                var queryMTDatabaseTemplate = GetActivityTemplate(activityList, "QueryMTDatabase");
                var queryMTDatabaseAction = await CreateQueryMTDatabaseAction(queryMTDatabaseTemplate, ++ordering);
                await SetQueryMTDatabaseActionFields(queryMTDatabaseAction);
                //let's make a followup configuration to fill criteria fields
                queryMTDatabaseAction = await ConfigureAction(queryMTDatabaseTemplate, queryMTDatabaseAction, null);
                actionDO.ChildNodes.Add(queryMTDatabaseAction);

                var filterUsingRuntimeTemplate = GetActivityTemplate(activityList, "FilterUsingRunTimeData");
                var filterAction = await CreateFilterUsingRunTimeAction(filterUsingRuntimeTemplate, ++ordering);
                actionDO.ChildNodes.Add(filterAction);
            }


            var notifierAction = await CreateNotifierAction(activityList, actionDO, howToBeNotifiedDdl, ++ordering);
            if (notifierAction != null)
            {
                actionDO.ChildNodes.Add(notifierAction);
            }

            return actionDO;
        }

        //TODO next 3 functions could be widely used in project
        private async Task<ActionDO> ConfigureAction(ActivityTemplateDTO template, ActionDO action, AuthorizationTokenDO authToken)
        {
            return await ExplicitConfigurationHelper.Configure(
                action,
                template,
                authToken
            );
        }

        private async Task<ActionDO> CreateAction(ActivityTemplateDTO template, string actionName, string actionLabel, int ordering, AuthorizationTokenDO authToken = null)
        {
            var action = new ActionDO
            {
                IsTempId = true,
                ActivityTemplateId = template.Id,
                CrateStorage = Crate.EmptyStorageAsStr(),
                CreateDate = DateTime.Now,
                Ordering = ordering,
                Name = actionName,
                Label = actionLabel
            };

            return await ConfigureAction(template, action, authToken);
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

        private async Task SetQueryMTDatabaseActionFields(ActionDO queryMTDatabase)
        {
            //update action's duration value
            using (var updater = Crate.UpdateStorage(queryMTDatabase))
            {
                var configControlCM = updater.CrateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .First();

                var objectList = (DropDownList)configControlCM.Controls.First(c => c.Name == "AvailableObjects");
                var docuSignEventObject = await GetDocuSignEventObject();
                objectList.Value = docuSignEventObject.Id.ToString(CultureInfo.InvariantCulture);
                objectList.selectedKey = docuSignEventObject.Name;

                var filterPane = (FilterPane)configControlCM.Controls.First(c => c.Name == "Filter");
                filterPane.Value = JsonConvert.SerializeObject(new FilterDataDTO
                {
                    ExecutionType = FilterExecutionType.WithFilter,
                    Conditions = new List<FilterConditionDTO> { new FilterConditionDTO{ Field = null, Operator = "", Value = null} }
                });
            }
        }

        private void SetDelayActionFields(ActionDO setDelayAction, Duration externalDurationControl)
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

        private async Task<ActionDO> CreateFilterUsingRunTimeAction(ActivityTemplateDTO template, int ordering)
        {
            return await CreateAction(template, "Filter Using Run Time", "Filter Using Run Time", ordering);
        }
        private async Task<ActionDO> CreateQueryMTDatabaseAction(ActivityTemplateDTO template, int ordering)
        {
            return await CreateAction(template, "Query MT Database", "Query MT Database", ordering);
        }

        private async Task<ActionDO> CreateSetDelayAction(ActivityTemplateDTO template, int ordering)
        {
            return await CreateAction(template, "Set Delay", "Set Delay", ordering);
        }

        private async Task<ActionDO> CreateMonitorDocuSignAction(ActivityTemplateDTO activity, AuthorizationTokenDO authTokenDO, int ordering)
        {
            return await CreateAction(activity, "Monitor DocuSign", "Monitor DocuSign", ordering, authTokenDO);
        }

        #endregion

        #region Monitor_DocuSign routines.
        private void SetMonitorDocuSignSpecificRecipient(ActionDO monitorAction, RadioButtonOption source)
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
            }
        }

        private void SetMonitorDocuSignSpecificTemplate(ActionDO monitorAction, RadioButtonOption option)
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
        }

        private async Task<ActionDO> CreateNotifierAction(IEnumerable<ActivityTemplateDTO> activityList, ActionDO solutionAction, DropDownList ddl, int ordering)
        {
            
            var notifierAction = (ActionDO)solutionAction.ChildNodes.FirstOrDefault(c => ((ActionDO)c).ActivityTemplate.Tags != null && ((ActionDO)c).ActivityTemplate.Tags.Contains("Notifier"));
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
                    var handlerAction = new ActionDO
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


        private async Task<MT_Object> GetDocuSignEventObject()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return await uow.MTObjectRepository.GetQuery().FirstOrDefaultAsync(o => o.ManifestId == (int)MT.DocuSignEvent);
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

        private async Task<Crate> PackAvailableRecipientEvents(ActionDO actionDO)
        {
            var events = new []{"Created", "Sent", "Delivered", "Signed", "Declined", "FaxPending", "AutoResponded"};

            var availableRecipientEventsCrate =
                Crate.CreateDesignTimeFieldsCrate(
                    "AvailableRecipientEvents", events.Select(x => new FieldDTO(x, x)).ToArray()
                );

            return availableRecipientEventsCrate;
        }

        private async Task<Crate> PackAvailableHandlers(ActionDO actionDO)
        {
            var templates = await HubCommunicator.GetActivityTemplates(actionDO);
            var taggedTemplates = templates.Where(x => x.Tags != null && x.Tags.Contains("Notifier"));

            var availableHandlersCrate =
                Crate.CreateDesignTimeFieldsCrate(
                    "AvailableHandlers",
                    taggedTemplates.Select(x => new FieldDTO(x.Label, x.Id.ToString())).ToArray()
                );

            return availableHandlersCrate;
        }

        #endregion Monitor_DocuSign routines.

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActionDO, containerId));
        }
    }
}