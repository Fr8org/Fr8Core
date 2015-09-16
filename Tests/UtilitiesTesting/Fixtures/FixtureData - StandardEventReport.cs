using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Wrappers;
using Newtonsoft.Json;
using StructureMap;
using System.Collections.Generic;
using System;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static CrateDTO RawStandardEventReportFormat()
        {
            return new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Standard Event Report",
                ManifestType = "Standard Event Report",
                Contents = @"{ EventNames : ""DocuSign Envelope Sent"", ProcessDOId: """", EventPayload: [ ]}"
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
