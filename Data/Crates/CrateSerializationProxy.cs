using System;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Data.Crates
{
    public class CrateSerializationProxy
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("contents")]
        public JToken Contents { get; set; }

        [JsonProperty("parentCrateId")]
        public string ParentCrateId { get; set; }

        [JsonProperty("manifestType")]
        public string ManifestType { get; set; }

        [JsonProperty("manifestId")]
        public int ManifestId { get; set; }

        [JsonProperty("manufacturer")]
        public ManufacturerDTO Manufacturer { get; set; }

        [JsonProperty("createTime")]
        public DateTime CreateTime { get; set; }
    }
}
