using System;
using Fr8.Infrastructure.Data.Convertors.JsonNet;
using Fr8.Infrastructure.Data.States;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class CrateDTO
    {
        [JsonProperty("manifestType")]
        public string ManifestType { get; set; }

        [JsonProperty("manifestId")]
        public int ManifestId { get; set; }

        [JsonProperty("manufacturer")]
        public ManufacturerDTO Manufacturer { get; set; }

        [JsonProperty("manifestRegistrar")]
        public string ManifestRegistrar
        {
            get { return "fr8.co/registry"; }
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("contents")]
        public JToken Contents { get; set; }

        [JsonProperty("parentCrateId")]
        public string ParentCrateId { get; set; }

        [JsonProperty("createTime")]
        [JsonConverter(typeof(CreateTimeConverter))]
        public DateTime CreateTime { get; set; }
        
        [JsonProperty("sourceActivityId")]
        public string SourceActivityId { get; set; }
    }
}
