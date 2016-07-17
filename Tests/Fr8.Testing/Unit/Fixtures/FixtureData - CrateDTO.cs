using System.Collections.Generic;
using System;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.States;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static List<Crate<KeyValueListCM>> TestCrateDTO1()
        {
            List<KeyValueDTO> fields = new List<KeyValueDTO>();
            fields.Add(new KeyValueDTO { Key = "Medical_Form_v1", Value = Guid.NewGuid().ToString() });
            fields.Add(new KeyValueDTO { Key = "Medical_Form_v2", Value = Guid.NewGuid().ToString() });

            return new List<Crate<KeyValueListCM>> { Crate<KeyValueListCM>.FromContent("Available Templates", new KeyValueListCM(fields)) };
        }

        public static List<Crate> TestCrateDTO2()
        {
            List<KeyValueDTO> fields = new List<KeyValueDTO>();

            fields.Add(new KeyValueDTO { Key = "Text 5", Value = Guid.NewGuid().ToString() });
            fields.Add(new KeyValueDTO { Key = "Text 8", Value = Guid.NewGuid().ToString() });
            fields.Add(new KeyValueDTO { Key = "Doctor", Value = Guid.NewGuid().ToString() });
            fields.Add(new KeyValueDTO { Key = "Condition", Value = Guid.NewGuid().ToString() });

            return new List<Crate> { Crate.FromContent("DocuSignTemplateUserDefinedFields", new KeyValueListCM { Values = fields }) };

        }

        public static List<Crate> TestCrateDTO3()
        {
            return new List<Crate>
            {
                Crate.FromContent("Crate1", new CrateDescriptionCM
                {
                    CrateDescriptions =
                    {
                        new CrateDescriptionDTO
                        {
                            Label = "Crate2",
                            Availability = AvailabilityType.Always,
                            Fields =
                            {
                                new FieldDTO("Text 5"),
                                new FieldDTO("Doctor"),
                                new FieldDTO("Condition"),
                            }
                        }
                    }
                }),
                Crate.FromContent("CrateId2", new StandardConfigurationControlsCM()),
                Crate.FromContent("CrateId3", new DocuSignRecipientCM()),
                Crate.FromContent("CrateId4", new EventSubscriptionCM()),
                Crate.FromContent("CrateId5", new StandardFileListCM())
            };
        }

        public static CrateDTO TestEmptyCrateDTO()
        {
            return new CrateDTO
            {
                Label = "Test",
                CreateTime = DateTime.Now,
                Id = "123"
            };
        }


        public static Crate CreateStandardConfigurationControls()
        {
            string templateId = "58521204-58af-4e65-8a77-4f4b51fef626";
            var fieldSelectDocusignTemplate = new DropDownList
            {
                Label = "Select DocuSign Template",
                Name = "Selected_DocuSign_Template",
                Required = true,
                Events = new List<ControlEvent>
                {
                    new ControlEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                },
                Value = templateId
            };

            var fieldEnvelopeSent = new CheckBox
            {
                Label = "Envelope Sent",
                Name = "Event_Envelope_Sent"
            };

            var fieldEnvelopeReceived = new CheckBox
            {
                Label = "Envelope Received",
                Name = "Event_Envelope_Received"
            };

            var fieldRecipientSigned = new CheckBox
            {
                Label = "Recipient Signed",
                Name = "Event_Recipient_Signed"
            };

            var fieldEventRecipientSent = new CheckBox
            {
                Label = "Recipient Sent",
                Name = "Event_Recipient_Sent"
            };


            return PackControlsCrate(
                fieldSelectDocusignTemplate,
                fieldEnvelopeSent,
                fieldEnvelopeReceived,
                fieldRecipientSigned,
                fieldEventRecipientSent);
        }

        public static Crate CreateStandardConfigurationControlSelectFr8Object(string selected)
        {
            var fieldSelectFr8Object = new DropDownList
            {
                Label = "Select Fr8 Object",
                Name = "Selected_Fr8_Object",
                Value = selected,
                Required = true,
                Events = new List<ControlEvent>
                         {
                    new ControlEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Select Fr8 Object",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            return PackControlsCrate(fieldSelectFr8Object);
        }

        #region Private Methods

        private static Crate PackControlsCrate(params ControlDefinitionDTO[] controlsList)
        {
            return Crate.FromContent("Configuration_Controls", new StandardConfigurationControlsCM(controlsList));
        }

        #endregion


    }
}