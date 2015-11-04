using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using DocuSign.Integrations.Client;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Services;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Monitor_DocuSign_v1 : BasePluginAction
    {
        private class ActionUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public DropDownListControlDefinitionDTO Selected_DocuSign_Template { get; private set; }

            public ActionUi()
            {
                Controls = new List<ControlDefinitionDTO>
                {
                    (Selected_DocuSign_Template = DocuSignManager.CreateDocuSignTemplatePicker(true)),
                   
                    (new CheckBoxControlDefinitionDTO
                    {
                        Label = "Envelope Sent",
                        Name = "Event_Envelope_Sent",
                        Events = new List<ControlEvent>
                        {
                            ControlEvent.RequestConfig
                        }
                    }),
                    
                    (new CheckBoxControlDefinitionDTO
                    {
                        Label = "Envelope Received",
                        Name = "Event_Envelope_Received",
                        Events = new List<ControlEvent>
                        {
                            ControlEvent.RequestConfig
                        }
                    }),
                    (new CheckBoxControlDefinitionDTO
                    {
                        Label = "Recipient Signed",
                        Name = "Event_Recipient_Signed",
                        Events = new List<ControlEvent>
                        {
                            ControlEvent.RequestConfig
                        }
                    }),

                    (new CheckBoxControlDefinitionDTO
                    {
                        Label = "Recipient Sent",
                        Name = "Event_Recipient_Sent",
                        Events = new List<ControlEvent>
                        {
                            ControlEvent.RequestConfig
                        }
                    })

                };
            }
        }


        readonly DocuSignManager _docuSignManager = new DocuSignManager();

        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            if (NeedsAuthentication(curActionDTO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActionDTO, x => ConfigurationEvaluator(x));
        }

        public ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            if (Crate.IsEmptyStorage(curActionDTO.CrateStorage))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected Crate PackCrate_DocuSignTemplateNames(DocuSignAuthDTO authDTO)
        {
            var template = new DocuSignTemplate();
            var templates = template.GetTemplates(authDTO.Email, authDTO.ApiPassword);
            var fields = templates.Select(x => new FieldDTO() { Key = x.Name, Value = x.Id }).ToArray();
            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate("Available Templates", fields);
            
            return createDesignTimeFields;
        }
        
        public object Activate(ActionDTO curDataPackage)
        {
            DocuSignAccount docuSignAccount = new DocuSignAccount();
            ConnectProfile connectProfile = docuSignAccount.GetDocuSignConnectProfiles();
            if (Int32.Parse(connectProfile.totalRecords) > 0)
            {
                return "Not Yet Implemented"; // Will be changed when implementation is plumbed in.
            }
            else
            {
                return "Fail";
            }
        }

        public object Deactivate(ActionDTO curDataPackage)
        {
            DocuSignAccount docuSignAccount = new DocuSignAccount();
            ConnectProfile connectProfile = docuSignAccount.GetDocuSignConnectProfiles();
            if (Int32.Parse(connectProfile.totalRecords) > 0)
            {
                return "Not Yet Implemented"; // Will be changed when implementation is plumbed in.
            }
            else
            {
                return "Fail";
            }
        }

        public async Task<PayloadDTO> Run(ActionDTO actionDto)
        {
            if (NeedsAuthentication(actionDto))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            var processPayload = await GetProcessPayload(actionDto.ProcessId);

            // Extract envelope id from the payload Crate
            string envelopeId = GetEnvelopeId(processPayload);

            // Make sure that it exists
            if (String.IsNullOrEmpty(envelopeId))
                throw new PluginCodedException(PluginErrorCode.PAYLOAD_DATA_MISSING, "EnvelopeId");

            //Create a field
            var fields = new List<FieldDTO>
            {
                new FieldDTO()
                {
                    Key = "EnvelopeId",
                    Value = envelopeId
                }
            };

            using (var updater = Crate.UpdateStorage(() => processPayload.CrateStorage))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Payload Data", new StandardPayloadDataCM(fields)));
            }

            return processPayload;
        }

        private string GetEnvelopeId(PayloadDTO curPayloadDTO)
        {
            var eventReportCrate = Crate.GetStorage(curPayloadDTO.CrateStorage).CratesOfType<EventReportCM>().SingleOrDefault();
            
            if (eventReportCrate == null)
            {
                return null;
            }
            
            var standardPayload = eventReportCrate.Value.EventPayload.CrateValuesOfType<StandardPayloadDataCM>().FirstOrDefault();

            if (standardPayload == null)
            {
                return null;
            }

            var envelopeId = standardPayload.GetValues("EnvelopeId").FirstOrDefault();
          
            return envelopeId;
        }

        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(curActionDTO.AuthToken.Token);
            
            using (var updater = Crate.UpdateStorage(() => curActionDTO.CrateStorage))
            {
                var controls = new ActionUi();
                updater.CrateStorage.Add(PackControls(controls));
                updater.CrateStorage.Add(_docuSignManager.PackCrate_DocuSignTemplateNames(docuSignAuthDTO));
                updater.CrateStorage.Add(PackCrate_DocuSignEventFields());

                updater.CrateStorage.RemoveByManifestId((int) MT.StandardEventSubscription);
                updater.CrateStorage.Add(PackCrate_EventSubscriptions(controls));
            }
           
            return await Task.FromResult(curActionDTO);
        }

        protected override Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            var storage = Crate.GetStorage(curActionDTO.CrateStorage);
            var controls = storage.CrateValuesOfType<StandardConfigurationControlsCM>().First();
            var ui = new ActionUi();

            ui.ClonePropertiesFrom(controls);
            
            string curSelectedTemplateId = ui.Selected_DocuSign_Template.Value;

            if (!string.IsNullOrWhiteSpace(curSelectedTemplateId))
            {
                //get the existing DocuSign event fields
                var curEventFields = storage.CratesOfType<StandardDesignTimeFieldsCM>().Single(x => x.Label == "DocuSign Event Fields").Value;
                
                //set the selected template ID
                curEventFields.Fields.ForEach(field =>
                {
                    if (field.Key.Equals("TemplateId"))
                    {
                        field.Value = curSelectedTemplateId;
                    }
                });

                using (var updater = Crate.UpdateStorage(() => curActionDTO.CrateStorage))
                {
                    updater.CrateStorage.RemoveByLabel("DocuSign Event Fields");
                    updater.CrateStorage.Add(Crate.CreateDesignTimeFieldsCrate("DocuSign Event Fields", curEventFields.Fields.ToArray()));

                    updater.CrateStorage.RemoveByManifestId((int)MT.StandardEventSubscription);
                    updater.CrateStorage.Add(PackCrate_EventSubscriptions(controls));
                }
            }

            return Task.FromResult(curActionDTO);
        }

     
        private Crate PackCrate_EventSubscriptions(StandardConfigurationControlsCM configurationFields)
        {
            var subscriptions = new List<string>();

            var eventCheckBoxes = configurationFields.Controls.Where(x => x.Type == "CheckBox" && x.Name.StartsWith("Event_"));

            foreach (var eventCheckBox in eventCheckBoxes)
            {
                if (eventCheckBox.Selected)
                {
                    subscriptions.Add(eventCheckBox.Label);
                }
            }

            return Crate.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                subscriptions.ToArray()
                );
        }

        private Crate PackCrate_DocuSignEventFields()
        {
            return Crate.CreateDesignTimeFieldsCrate("DocuSign Event Fields",
                new FieldDTO { Key = "EnvelopeId", Value = string.Empty },
                new FieldDTO { Key = "TemplateId", Value = string.Empty });
        }
    }
}
