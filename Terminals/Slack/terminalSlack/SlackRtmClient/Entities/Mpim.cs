using Newtonsoft.Json;

namespace terminalSlack.RtmClient.Entities
{
    public class Mpim
    {
        public string Id { get; set; }

        public string Name { get; set; }
        [JsonProperty("creator")]
        public string CreatorId { get; set; }
    }
}
