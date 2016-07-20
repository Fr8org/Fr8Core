using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.DataTransferObjects
{
    public class ManifestDescriptionDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("sampleJSON")]
        public string SampleJSON { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("registeredBy")]
        public string RegisteredBy { get; set; }

    }
}
