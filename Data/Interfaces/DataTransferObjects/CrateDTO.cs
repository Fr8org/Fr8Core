using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Data.States;

namespace Data.Interfaces.DataTransferObjects
{
    public class CrateDTO
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

        [JsonProperty("availability")]
        public AvailabilityType Availability { get; set; }
    }
}
