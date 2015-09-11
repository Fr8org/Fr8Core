using Data.Entities;
using PluginBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;
using Core.Interfaces;
using StructureMap;
using Newtonsoft.Json;
using Data.Wrappers;
using Data.Interfaces;

namespace pluginDocuSign.Actions
{
    public class Wait_For_DocuSign_Event_v1 : BasePluginAction
    {
        IAction _action = ObjectFactory.GetInstance<IAction>();
        ICrate _crate = ObjectFactory.GetInstance<ICrate>();
        IDocuSignTemplate _template = ObjectFactory.GetInstance<IDocuSignTemplate>();
        IEnvelope _docusignEnvelope = ObjectFactory.GetInstance<IEnvelope>();

        public object Configure(ActionDO curActionDO, bool forceFollowupConfiguration = false)
        {
            //TODO: The coniguration feature for Docu Sign is not yet defined. The configuration evaluation needs to be implemented.
            return ProcessConfigurationRequest(curActionDO, 
                actionDo => (forceFollowupConfiguration) ? 
                    ConfigurationRequestType.Followup : 
                    ConfigurationRequestType.Initial); // will be changed to complete the config feature for docu sign
        }

        public object Activate(ActionDO curActionDO)
        {
            return "Activate Request"; // Will be changed when implementation is plumbed in.
        }

        public object Execute(ActionDO curActionDO)
        {
            return "Execute Request"; // Will be changed when implementation is plumbed in.
        }

        protected override CrateStorageDTO InitialConfigurationResponse(ActionDO curActionDO)
        {
            var fieldSelectDockusignTemplate = new FieldDefinitionDTO()
            {
                FieldLabel = "Select DocuSign Template",
                Type = "dropdownlistField",
                Name = "Selected_DocuSign_Template",
                Required = true,
                Events = new List<FieldEvent>() {
                     new FieldEvent("onSelect", "requestConfiguration")
                }
            };

            var fieldEnvelopeSent = new FieldDefinitionDTO()
            {
                FieldLabel = "Envelope Sent",
                Type = "checkboxField",
                Name = "Event_Envelope_Sent"
            };

            var fieldEnvelopeReceived = new FieldDefinitionDTO()
            {
                FieldLabel = "Envelope Received",
                Type = "checkboxField",
                Name = "Event_Envelope_Received"
            };

            var fieldRecipientSigned = new FieldDefinitionDTO()
            {
                FieldLabel = "Recipient Signed",
                Type = "checkboxField",
                Name = "Event_Recipient_Signed"
            };

            var fieldEventRecipientSent = new FieldDefinitionDTO()
            {
                FieldLabel = "Recipient Sent",
                Type = "checkboxField",
                Name = "Event_Recipient_Sent"
            };

            var crateConfiguration = new List<CrateDTO>()
            {
                _crate.Create("Selected_DocuSign_Template", JsonConvert.SerializeObject(fieldSelectDockusignTemplate)),
                _crate.Create("Event_Envelope_Sent", JsonConvert.SerializeObject(fieldEnvelopeSent)),
                _crate.Create("Event_Envelope_Received", JsonConvert.SerializeObject(fieldEnvelopeReceived)),
                _crate.Create("Event_Recipient_Signed", JsonConvert.SerializeObject(fieldRecipientSigned)),
                _crate.Create("Event_Recipient_Sent", JsonConvert.SerializeObject(fieldEventRecipientSent))
            };

            _action.AddCrate(curActionDO, crateConfiguration);
            return curActionDO.CrateStorageDTO();
        }

        protected override CrateStorageDTO FollowupConfigurationResponse(ActionDO curActionDO)
        {
            var crateStorage = curActionDO.CrateStorageDTO().CratesDTO;

            if (crateStorage == null || crateStorage.Count == 0)
            {
                return curActionDO.CrateStorageDTO();
            }

            // Extract DocuSign Template Id
            var docusignTemplateCrate = crateStorage.SingleOrDefault(c => c.Label == "Selected_DocuSign_Template");

            if (docusignTemplateCrate == null || String.IsNullOrEmpty(docusignTemplateCrate.Contents))
            {
                return curActionDO.CrateStorageDTO();
            }

            var docusignTemplateField = JsonConvert.DeserializeObject<FieldDefinitionDTO>(docusignTemplateCrate.Contents);

            if (docusignTemplateField == null || string.IsNullOrEmpty(docusignTemplateField.Value))
            {
                return curActionDO.CrateStorageDTO();
            }

            var docusignTemplateId = docusignTemplateField.Value;
            var userDefinedFields = _docusignEnvelope.GetEnvelopeData(docusignTemplateId);
            var crateConfiguration = new List<CrateDTO>();
            var fieldCollection = userDefinedFields.Select(f => new FieldDefinitionDTO()
            {
                FieldLabel = f.Name,
                Type = "textboxField",
                Name = f.Name,
                Value = f.Value
            });

            crateConfiguration.Add(_crate.Create(
                "DocuSignTemplateUserDefinedFields", 
                JsonConvert.SerializeObject(fieldCollection), 
                "DocuSignTemplateUserDefinedFields"));

            //crateConfiguration.Add(_crate.Create(
            //    "DocuSignEnvelopeStandardFields", 
            //    JsonConvert.SerializeObject(fieldCollection), 
            //    "DocuSignEnvelopeStandardFields"));

            _action.AddCrate(curActionDO, crateConfiguration);
            return curActionDO.CrateStorageDTO();
        }
    }
}