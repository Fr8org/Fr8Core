using Data.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Newtonsoft.Json;
using StructureMap;
using System.Collections.Generic;
using System;
using System.Linq;
using Data.Interfaces.ManifestSchemas;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {


        public static List<CrateDTO> TestCrateDTO1()
        {
            List<FieldDTO> fields = new List<FieldDTO>();
            fields.Add(new FieldDTO() { Key = "Medical_Form_v1", Value = Guid.NewGuid().ToString() });
            fields.Add(new FieldDTO() { Key = "Medical_Form_v2", Value = Guid.NewGuid().ToString() });

            CrateDTO curCrateDTO = new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Available Templates",
                Contents = JsonConvert.SerializeObject(new StandardDesignTimeFieldsCM() { Fields = fields }),
                ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME,
                ManifestId = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_ID

            };

            return new List<CrateDTO>() { curCrateDTO };

        }

        public static List<CrateDTO> TestCrateDTO2()
        {
            List<FieldDTO> fields = new List<FieldDTO>();
            fields.Add(new FieldDTO() { Key = "Text 5", Value = Guid.NewGuid().ToString() });
            fields.Add(new FieldDTO() { Key = "Text 8", Value = Guid.NewGuid().ToString() });
            fields.Add(new FieldDTO() { Key = "Doctor", Value = Guid.NewGuid().ToString() });
            fields.Add(new FieldDTO() { Key = "Condition", Value = Guid.NewGuid().ToString() });


            CrateDTO curCrateDTO = new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "DocuSignTemplateUserDefinedFields",
                Contents = JsonConvert.SerializeObject(new StandardDesignTimeFieldsCM() { Fields = fields }),
                ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME,
                ManifestId = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_ID

            };

            return new List<CrateDTO>() { curCrateDTO };

        }


        public static CrateDTO CreateStandardConfigurationControls()
        {
            string templateId = "58521204-58af-4e65-8a77-4f4b51fef626";
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
                },
                Value = templateId
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

            return PackControlsCrate(
                fieldSelectDocusignTemplate,
                fieldEnvelopeSent,
                fieldEnvelopeReceived,
                fieldRecipientSigned,
                fieldEventRecipientSent);
        }

        #region Private Methods

        private static CrateDTO PackControlsCrate(params ControlDefinitionDTO[] controlsList)
        {
            var controlsCrate = CreateStandardConfigurationControlsCrate(
                "Configuration_Controls", controlsList);

            return controlsCrate;
        }

        private static CrateDTO CreateStandardConfigurationControlsCrate(string label, params ControlDefinitionDTO[] controls)
        {
            return Create(label,
                JsonConvert.SerializeObject(new StandardConfigurationControlsCM() { Controls = controls.ToList() }),
                manifestType: CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME,
                manifestId: CrateManifests.STANDARD_CONF_CONTROLS_MANIFEST_ID);
        }

        private static CrateDTO Create(string label, string contents, string manifestType = "", int manifestId = 0)
        {
            var crateDTO = new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = label,
                Contents = contents,
                ManifestType = manifestType,
                ManifestId = manifestId
            };
            return crateDTO;
        }



        private static CrateDTO CreateEventSubscriptionCrate(
          StandardConfigurationControlsCM configurationFields)
        {
            var subscriptions = new List<string>();

            var eventCheckBoxes = configurationFields.Controls
                .Where(x => x.Type == "checkboxField" && x.Name.StartsWith("Event_"));

            foreach (var eventCheckBox in eventCheckBoxes)
            {
                if (eventCheckBox.Selected)
                {
                    subscriptions.Add(eventCheckBox.Label);
                }
            }

            return CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                subscriptions.ToArray()
                );
        }

        private static CrateDTO CreateStandardEventSubscriptionsCrate(string label, params string[] subscriptions)
        {
            return Create(label,
                JsonConvert.SerializeObject(new EventSubscriptionCM() { Subscriptions = subscriptions.ToList() }),
                manifestType: CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_NAME,
                manifestId: CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_ID);
        }

        #endregion


    }
}