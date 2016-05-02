using Newtonsoft.Json;

namespace terminalSlack.RtmClient.Entities
{
    public class Im
    {
        public string Id { get; set; }

        [JsonProperty("user")]
        public string UserId { get; set; }
    }
}
