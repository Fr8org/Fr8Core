using Newtonsoft.Json;

namespace terminalSlack.RtmClient.Entities
{
    public class Group
    {
        public string Id { get; set; }

        public string Name { get; set; }
        [JsonProperty("creator")]
        public string CreatorId { get; set; }
        [JsonProperty("is_archived")]
        public bool IsArchived { get; set; }
    }
}
