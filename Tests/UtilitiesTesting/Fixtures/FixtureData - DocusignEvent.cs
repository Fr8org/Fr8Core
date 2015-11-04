using System;
using Data.Entities;
using Data.States;
using DocuSign.Integrations.Client;
using Data.Interfaces.DataTransferObjects;
using System.Collections.Generic;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static CrateDTO TestDocuSignEventCrate()
        {
            var crateFields = new List<FieldDTO>()
                    {
                        new FieldDTO () { Key = "EnvelopeId", Value = "36" },
                        new FieldDTO() { Key = "ExternalEventType", Value = "1" },
                        new FieldDTO() {Key = "RecipientId", Value = "TestRecipientId" }
                    };
            var curEventData = new CrateDTO()
            {
                Contents = Newtonsoft.Json.JsonConvert.SerializeObject(crateFields),
                Label = "Event Data",
                Id = Guid.NewGuid().ToString()
            };
            return curEventData;
        }

    }
}