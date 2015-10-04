using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Data.Interfaces.ManifestSchemas;
﻿using System.Linq;


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
                Contents = JsonConvert.SerializeObject(new StandardDesignTimeFieldsMS() { Fields = fields }),
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
                Contents = JsonConvert.SerializeObject(new StandardDesignTimeFieldsMS() { Fields = fields }),
                ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME,
                ManifestId = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_ID

            };

            return new List<CrateDTO>() { curCrateDTO };

        }


        public static CrateDTO CreateStandardConfigurationControls()
        {
            string templateId = "58521204-58af-4e65-8a77-4f4b51fef626";
            var fieldSelectDocusignTemplate = new DropdownListFieldDefinitionDTO()
            {
                Label = "Select DocuSign Template",
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
                },
                Value = templateId
            };

            var fieldEnvelopeSent = new CheckBoxFieldDefinitionDTO()
            {
                Label = "Envelope Sent",
                Name = "Event_Envelope_Sent"
            };

            var fieldEnvelopeReceived = new CheckBoxFieldDefinitionDTO()
            {
                Label = "Envelope Received",
                Name = "Event_Envelope_Received"
            };

            var fieldRecipientSigned = new CheckBoxFieldDefinitionDTO()
            {
                Label = "Recipient Signed",
                Name = "Event_Recipient_Signed"
            };

            var fieldEventRecipientSent = new CheckBoxFieldDefinitionDTO()
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

        private static CrateDTO PackControlsCrate(params FieldDefinitionDTO[] controlsList)
        {
            var controlsCrate = CreateStandardConfigurationControlsCrate(
                "Configuration_Controls", controlsList);

            return controlsCrate;
        }

        private static CrateDTO CreateStandardConfigurationControlsCrate(string label, params FieldDefinitionDTO[] controls)
        {
            return Create(label,
                JsonConvert.SerializeObject(new StandardConfigurationControlsMS() { Controls = controls.ToList() }),
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
          StandardConfigurationControlsMS configurationFields)
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
                JsonConvert.SerializeObject(new EventSubscriptionMS() { Subscriptions = subscriptions.ToList() }),
                manifestType: CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_NAME,
                manifestId: CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_ID);
        }

        #endregion


    }
}