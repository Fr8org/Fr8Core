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
        public static ControlDefinitionDTO[] FieldDefinitionDTO1()
        {
            var fieldSelectDocusignTemplate = new DropDownListControlDefinitionDTO()
            {
                Label = "Select DocuSign Template",
                Name = "Selected_DocuSign_Template",
                Required = true,
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                }
            };

            var fieldEnvelopeSent = new CheckBoxControlDefinitionDTO()
            {
                Label = "Envelope Sent",
                Name = "Event_Envelope_Sent"
            };

            var fieldEnvelopeReceived = new CheckBoxControlDefinitionDTO()
            {
                Label = "Envelope Received",
                Name = "Event_Envelope_Received"
            };

            var fieldRecipientSigned = new CheckBoxControlDefinitionDTO()
            {
                Label = "Recipient Signed",
                Name = "Event_Recipient_Signed"
            };

            var fieldEventRecipientSent = new CheckBoxControlDefinitionDTO()
            {
                Label = "Recipient Sent",
                Name = "Event_Recipient_Sent"
            };

            return new ControlDefinitionDTO[] {
                fieldSelectDocusignTemplate,
                fieldEnvelopeSent,
                fieldEnvelopeReceived,
                fieldRecipientSigned,
                fieldEventRecipientSent };
        }
    }
}