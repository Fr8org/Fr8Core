using System;
using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hub.Managers.Crates
{
    partial class CrateStorageSerializer
    {
        public class CrateStorageSerializationProxy
        {
            [JsonProperty("crates")]
            public List<CrateSerializationProxy> Crates { get; set; }
        }

        public class CrateSerializationProxy
        {
            public string Id { get; set; }

            public string Label { get; set; }

            public JToken Contents { get; set; }

            public string ParentCrateId { get; set; }

            public string ManifestType { get; set; }

            public int ManifestId { get; set; }

            public ManufacturerDTO Manufacturer { get; set; }

            public DateTime CreateTime { get; set; }
        }
    }
}
