using System;
using Data.Entities;
using Data.States;
using Data.Wrappers;
using DocuSign.Integrations.Client;
using Data.Interfaces.DataTransferObjects;
using System.Collections.Generic;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static DocuSignEventDO TestDocuSignEvent1()
        {
            return new DocuSignEventDO
            {
                   EnvelopeId    = "36",
                   ExternalEventType = ExternalEventType.EnvelopeSent
            };
        }

        public static CrateDTO DocuSignEventToCrate(DocuSignEventDO curEvent)
        {
            var crateFields = new List<FieldDTO>()
                    {
                        new FieldDTO () { Key = "EnvelopeId", Value = curEvent.EnvelopeId },
                        new FieldDTO() { Key = "ExternalEventType", Value = curEvent.ExternalEventType.ToString() },
                        new FieldDTO() {Key = "RecipientId", Value = curEvent.RecipientId.ToString() }
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