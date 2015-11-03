using System;
using Data.Entities;
using Data.States;
using DocuSign.Integrations.Client;
using Data.Interfaces.DataTransferObjects;
using System.Collections.Generic;
using Data.Crates;
using Newtonsoft.Json.Linq;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static DocuSignEventDO TestDocuSignEvent1()
        {
            return new DocuSignEventDO
            {
                   EnvelopeId    = "36",
                   ExternalEventType = 1,
                   RecipientId = "TestRecipientId"
                   
            };
        }

        
        public static Crate DocuSignEventToCrate(DocuSignEventDO curEvent)
        {
            var crateFields = new List<FieldDTO>()
                    {
                        new FieldDTO () { Key = "EnvelopeId", Value = curEvent.EnvelopeId },
                        new FieldDTO() { Key = "ExternalEventType", Value = curEvent.ExternalEventType.ToString() },
                        new FieldDTO() {Key = "RecipientId", Value = curEvent.RecipientId.ToString() }
                    };

            return Crate.FromJson("Event Data", JToken.FromObject(crateFields));
        }

    }
}