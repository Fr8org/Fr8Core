using Newtonsoft.Json;

namespace terminalStatX.DataTransferObjects
{
    public class StatXGroupDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}