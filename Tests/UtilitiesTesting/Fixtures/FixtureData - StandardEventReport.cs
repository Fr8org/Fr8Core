using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Wrappers;
using Newtonsoft.Json;
using StructureMap;
using System.Collections.Generic;
using System;
using Utilities.Serializers.Json;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static CrateDTO RawStandardEventReportFormat()
        {
            var serializer = new Utilities.Serializers.Json.JsonSerializer();
            EventReportMS eventReportMS = new EventReportMS()
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

        public static EventReportMS StandardEventReportFormat()
        {
            return new EventReportMS()
            {
                EventNames = "DocuSign Envelope Sent"
            };
        }
    }
}
