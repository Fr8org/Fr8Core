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
            var fieldSelectDocusignTemplate = new FieldDefinitionDTO()
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

            var fields = new List<FieldDefinitionDTO>()
            {
                fieldSelectDocusignTemplate,
                fieldEnvelopeSent,
                fieldEnvelopeReceived,
                fieldRecipientSigned,
                fieldEventRecipientSent
            };

            var crateConfiguration = new List<CrateDTO>()
            {
                _crate.Create("Configuration_Controls", JsonConvert.SerializeObject(fields)),
            };

            _action.AddCrate(curActionDO, crateConfiguration);
            return curActionDO.CrateStorageDTO();
        }

        protected override CrateStorageDTO FollowupConfigurationResponse(ActionDO curActionDO)
        {
            var curCrates = _action.GetCrates(curActionDO);

            if (curCrates == null || curCrates.Count == 0)
            {
                return curActionDO.CrateStorageDTO();
            }

            // Extract DocuSign Template Id
            var configurationFieldsCrate = curCrates.SingleOrDefault(c => c.Label == "Configuration_Controls");

            if (configurationFieldsCrate == null || String.IsNullOrEmpty(configurationFieldsCrate.Contents))
            {
                return curActionDO.CrateStorageDTO();
            }

            var configurationFields = JsonConvert.DeserializeObject<List<FieldDefinitionDTO>>(configurationFieldsCrate.Contents);

            if (configurationFields == null || !configurationFields.Any(c => c.Name == "Selected_DocuSign_Template"))
            {
                return curActionDO.CrateStorageDTO();
            }

            var docusignTemplateId = configurationFields.SingleOrDefault(c => c.Name == "Selected_DocuSign_Template").Value;
            var userDefinedFields = _docusignEnvelope.GetEnvelopeDataByTemplate(docusignTemplateId);
            var crateConfiguration = new List<CrateDTO>();
            var fieldCollection = userDefinedFields.Select(f => new FieldDefinitionDTO()
            {
                FieldLabel = f.Name,
                Type = f.Type,
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