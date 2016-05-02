using Newtonsoft.Json;

namespace terminalSlack.RtmClient.Events
{
    public abstract class EventBase
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
