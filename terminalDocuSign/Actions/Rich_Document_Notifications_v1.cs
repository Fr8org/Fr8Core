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
                    Name = "SpecificTemplate",
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
                            Value = "When the event happens"
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

        public DocuSignManager DocuSignManager { get; set; }

        public Rich_Document_Notifications_v1()
        {
            DocuSignManager = new DocuSignManager();
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

        protected override Task<ActionDO> FollowupConfigurationResponse(
            ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            throw new NotImplementedException();
        }

        private Crate PackAvailableTemplates(AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert
                .DeserializeObject<DocuSignAuthDTO>(authTokenDO.Token);

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
            var taggedTemplates = templates.Where(x => x.Tags.Contains("Notifier"));

            var availableHandlersCrate =
                Crate.CreateDesignTimeFieldsCrate(
                    "AvailableActions",
                    templates.Select(x => new FieldDTO(x.Label, x.Id.ToString())).ToArray()
                );

            return availableHandlersCrate;
        }
    }
}