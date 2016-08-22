using Newtonsoft.Json;

namespace terminalSlack.RtmClient.Events
{
    public class MessageBase : EventBase
    {
        public string Subtype { get; set; }

        [JsonProperty("ts")]
        public string Timestamp { get; set; }
        [JsonProperty("channel")]
        public string ChannelId { get; set; }

        [JsonProperty("user")]
        public string UserId { get; set; }
        [JsonProperty("hidden")]
        public bool IsHidden { get; set; }
    }
}