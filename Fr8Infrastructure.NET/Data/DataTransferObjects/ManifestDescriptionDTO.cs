using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
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
