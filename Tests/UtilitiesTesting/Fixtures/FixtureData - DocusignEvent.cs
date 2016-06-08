using System.Collections.Generic;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json.Linq;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static Crate TestDocuSignEventCrate()
        {
            var crateFields = new List<FieldDTO>()
                    {
                        new FieldDTO () { Key = "EnvelopeId", Value = "36" },
                        new FieldDTO() { Key = "ExternalEventType", Value = "1" },
                        new FieldDTO() {Key = "RecipientId", Value = "TestRecipientId" }
                    };
            
            return Crate.FromJson("Event Data", JToken.FromObject(crateFields));
        }

    }
}