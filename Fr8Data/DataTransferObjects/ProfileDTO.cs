using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class ProfileDTO
    { 
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
