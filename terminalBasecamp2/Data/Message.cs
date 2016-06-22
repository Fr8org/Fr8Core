using Newtonsoft.Json;

namespace terminalBasecamp2.Data
{
    public class Message
    {
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}