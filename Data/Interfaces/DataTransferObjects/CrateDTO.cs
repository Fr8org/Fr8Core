using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class CrateDTO
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("contents")]
        public string Contents { get; set; }

        [JsonProperty("parentCrateId")]
        public string ParentCrateId { get; set; }

        [JsonProperty("manifestType")]
        public string ManifestType { get; set; }

        [JsonProperty("manifestId")]
        public int ManifestId { get; set; }

        [JsonProperty("manufacturer")]
        public ManufacturerDTO Manufacturer { get; set; }
    }
}
