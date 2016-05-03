using Newtonsoft.Json;

namespace terminalSlack.RtmClient.Entities
{
    public class Self
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
