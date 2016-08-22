using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.Control;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static ControlDefinitionDTO[] FieldDefinitionDTO1()
        {
            var fieldSelectDocusignTemplate = new DropDownList()
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
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            var fieldEnvelopeSent = new CheckBox()
            {
                Label = "Envelope Sent",
                Name = "Event_Envelope_Sent"
            };

            var fieldEnvelopeReceived = new CheckBox()
            {
                Label = "Envelope Received",
                Name = "Event_Envelope_Received"
            };

            var fieldRecipientSigned = new CheckBox()
            {
                Label = "Recipient Signed",
                Name = "Event_Recipient_Signed"
            };

            var fieldEventRecipientSent = new CheckBox()
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