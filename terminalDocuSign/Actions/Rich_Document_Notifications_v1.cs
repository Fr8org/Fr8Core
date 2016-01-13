using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalDocuSign.Infrastructure;

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
                    Label = "What will this notification track?",
                    Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") },
                    Radios = new List<RadioButtonOption>
                    {
                        new RadioButtonOption
                        {
                            Name = "SpecificRecipient",
                            Value = "A specific Recipient",
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
                            Value = "A specific Template",
                            Controls = new List<ControlDefinitionDTO>
                            {
                                new DropDownList()
                                {
                                    Name = "SpecificTemplate",
                                    Source = new FieldSourceDTO
                                    {
                                        Label = "AvailableTemplates",
                                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                                    },
                                    Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
                                }
                            }
                        }
                    }
                });

                Controls.Add(new DropDownList()
                {
                    Name = "SpecificEvent",
                    Label = "What event do you want to be notified about?",
                    Source = new FieldSourceDTO
                    {
                        Label = "AvailableEvents",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    },
                    Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
                });

                Controls.Add(new RadioButtonGroup()
                {
                    Label = "When do you want to be notified?",
                    Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") },
                    Radios = new List<RadioButtonOption>
                    {
                        new RadioButtonOption
                        {
                            Name = "NotificationMode",
                            Value = "When the event happens",
                            Selected = true
                        }
                    }
                });

                Controls.Add(new DropDownList()
                {
                    Name = "NotificationHandler",
                    Label = "How do you want to be notified?",
                    Source = new FieldSourceDTO
                    {
                        Label = "AvailableHandlers",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    },
                    Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
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

        protected override async Task<ActionDO> InitialConfigurationResponse(
            ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(actionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.Add(PackAvailableTemplates(authTokenDO));
                updater.CrateStorage.Add(PackAvailableEvents());
                updater.CrateStorage.Add(await PackAvailableHandlers(actionDO));
            }

            return actionDO;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(
            ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            var controls = Crate.GetStorage(actionDO)
                .CrateContentsOfType<StandardConfigurationControlsCM>()
                .First();

            var specificRecipientOption = ((RadioButtonGroup)controls.Controls[0]).Radios[0];
            var specificTemplateOption = ((RadioButtonGroup)controls.Controls[0]).Radios[1];
            var specificEventDdl = (DropDownList)controls.Controls[1];
            var specificHandlerDdl = (DropDownList)controls.Controls[3];

            if (actionDO.ChildNodes == null || actionDO.ChildNodes.Count == 0)
            {
                await CreateEmptyMonitorDocuSignAction(actionDO, authTokenDO);
            }

            if (specificRecipientOption.Selected)
            {
                ApplyMonitorDocuSignSpecificRecipient((ActionDO)actionDO.ChildNodes[0], specificRecipientOption);
            }
            else if (specificTemplateOption.Selected)
            {
                ApplyMonitorDocuSignSpecificTemplate((ActionDO)actionDO.ChildNodes[0], specificTemplateOption);
            }

            ApplyMonitorDocuSignSpecificEvent((ActionDO)actionDO.ChildNodes[0], specificEventDdl);

            await ApplyHandlerAction(actionDO, specificHandlerDdl);

            return actionDO;
        }

        public override Task<ActionDO> Activate(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //create DocuSign account if there is no existing connect profile
            var docuSignAccount = new DocuSignAccount();
            DocuSignAccount.CreateOrUpdateDefaultDocuSignConnectConfiguration(docuSignAccount, null);

            return Task.FromResult<ActionDO>(curActionDO);
        }

        #region Monitor_DocuSign routines.

        private async Task CreateEmptyMonitorDocuSignAction(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            actionDO.ChildNodes = new List<RouteNodeDO>();

            const string monitorDocuSignTemplateName = "Monitor_DocuSign_Envelope_Activity";
            var monitorDocuSignTemplate = (await HubCommunicator.GetActivityTemplates(actionDO))
                .FirstOrDefault(x => x.Name == monitorDocuSignTemplateName);

            if (monitorDocuSignTemplate == null)
            {
                throw new Exception(string.Format("ActivityTemplate {0} was not found", monitorDocuSignTemplateName));
            }

            var monitorDocuSignAction = new ActionDO
            {
                IsTempId = true,
                ActivityTemplateId = monitorDocuSignTemplate.Id,
                CrateStorage = Crate.EmptyStorageAsStr(),
                CreateDate = DateTime.Now,
                Ordering = 1,
                Name = "Monitor DocuSign",
                Label = "Monitor DocuSign"
            };

            monitorDocuSignAction = await ExplicitConfigurationHelper.Configure(
                monitorDocuSignAction,
                monitorDocuSignTemplate,
                authTokenDO
            );

            actionDO.ChildNodes.Add(monitorDocuSignAction);
        }

        private void ApplyMonitorDocuSignSpecificRecipient(
            ActionDO monitorAction, RadioButtonOption source)
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

        private void ApplyMonitorDocuSignSpecificTemplate(
            ActionDO monitorAction, RadioButtonOption option)
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

        private void ApplyMonitorDocuSignSpecificEvent(
            ActionDO monitorAction, DropDownList ddl)
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

        private async Task ApplyHandlerAction(
            ActionDO solutionAction, DropDownList ddl)
        {
            // Remove action if action types do not match.
            if (solutionAction.ChildNodes != null && solutionAction.ChildNodes.Count == 2)
            {
                var handlerAction = (ActionDO)solutionAction.ChildNodes[1];
                var handlerActivityTemplateIdStr = handlerAction.ActivityTemplateId.HasValue
                    ? handlerAction.ActivityTemplateId.Value.ToString()
                    : string.Empty;

                if (handlerActivityTemplateIdStr != ddl.Value)
                {
                    solutionAction.ChildNodes.RemoveAt(1);
                }
            }

            // Add action if no notifier action exist.
            if (solutionAction.ChildNodes != null
                && solutionAction.ChildNodes.Count == 1
                && !string.IsNullOrEmpty(ddl.Value))
            {
                var templates = await HubCommunicator.GetActivityTemplates(solutionAction);
                var selectedTemplate = templates.FirstOrDefault(x => x.Id.ToString() == ddl.Value);

                if (selectedTemplate != null)
                {
                    var handlerAction = new ActionDO
                    {
                        IsTempId = true,
                        ActivityTemplateId = selectedTemplate.Id,
                        CrateStorage = Crate.EmptyStorageAsStr(),
                        CreateDate = DateTime.Now,
                        Ordering = 1,
                        Name = selectedTemplate.Label,
                        Label = selectedTemplate.Label
                    };

                    solutionAction.ChildNodes.Add(handlerAction);
                }
            }
        }

        #endregion Monitor_DocuSign routines.

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
                new FieldDTO { Key = "Envelope Sent", Value = "Event_Envelope_Sent" },
                new FieldDTO { Key = "Envelope Received", Value = "Event_Envelope_Received" },
                new FieldDTO { Key = "Recipient Signed", Value = "Event_Recipient_Signed" },
                new FieldDTO { Key = "Recipient Sent", Value = "Event_Recipient_Sent" }
            );

            return crate;
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

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActionDO, containerId));
        }
    }
}