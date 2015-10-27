using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Data.States;
using Hub.Interfaces;
using Utilities.Serializers.Json;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static CrateDTO RawStandardEventReportFormat()
        {
            var serializer = new Utilities.Serializers.Json.JsonSerializer();
            EventReportCM eventReportMS = new EventReportCM()
            {
                EventNames = "DocuSign Envelope Sent"
            };
            var eventReportJSON = serializer.Serialize(eventReportMS);
            return new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Standard Event Report",
                ManifestType = "Standard Event Report",
                Contents = eventReportJSON
            };
        }

        public static EventReportCM StandardEventReportFormat()
        {
            return new EventReportCM()
            {
                EventNames = "DocuSign Envelope Sent"
            };
        }
    }
}
