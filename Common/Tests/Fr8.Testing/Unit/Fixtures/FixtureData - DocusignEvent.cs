using System.Collections.Generic;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json.Linq;

namespace Fr8.Testing.Unit.Fixtures
{
    public partial class FixtureData
    {
        public static Crate TestDocuSignEventCrate()
        {
            var crateFields = new List<KeyValueDTO>()
                    {
                        new KeyValueDTO () { Key = "EnvelopeId", Value = "36" },
                        new KeyValueDTO() { Key = "ExternalEventType", Value = "1" },
                        new KeyValueDTO() {Key = "RecipientId", Value = "TestRecipientId" }
                    };
            
            return Crate.FromJson("Event Data", JToken.FromObject(crateFields));
        }

    }
}