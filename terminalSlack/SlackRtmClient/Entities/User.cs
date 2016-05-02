using Newtonsoft.Json;

namespace terminalSlack.RtmClient.Entities
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        [JsonProperty("deleted")]
        public bool IsDeleted { get; set; }
    }
}
