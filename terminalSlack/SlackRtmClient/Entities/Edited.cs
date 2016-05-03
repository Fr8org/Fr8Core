using Newtonsoft.Json;

namespace terminalSlack.RtmClient.Entities
{
    public class Edited
    {
        [JsonProperty("user")]
        public string UserId { get; set; }
        [JsonProperty("ts")]
        public string Timestamp { get; set; }
    }
}
