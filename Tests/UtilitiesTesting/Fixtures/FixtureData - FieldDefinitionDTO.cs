﻿using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static FieldDefinitionDTO[] FieldDefinitionDTO1()
        {
            var fieldSelectDocusignTemplate = new DropdownListFieldDefinitionDTO()
            {
                FieldLabel = "Select DocuSign Template",
                Type = "dropdownlistField",
                Name = "Selected_DocuSign_Template",
                Required = true,
                Events = new List<FieldEvent>()
                {
                    new FieldEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
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

            return new FieldDefinitionDTO[] {
                fieldSelectDocusignTemplate,
                fieldEnvelopeSent,
                fieldEnvelopeReceived,
                fieldRecipientSigned,
                fieldEventRecipientSent };
        }
    }
}