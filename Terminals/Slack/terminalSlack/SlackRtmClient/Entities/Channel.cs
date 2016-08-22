using Newtonsoft.Json;

namespace terminalSlack.RtmClient.Entities
{
    public class Channel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        [JsonProperty("is_archived")]
        public bool IsArchived { get; set; }
    }
}
