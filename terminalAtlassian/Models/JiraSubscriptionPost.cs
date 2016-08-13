using Newtonsoft.Json;

namespace terminalAtlassian.Models
{
    public class JiraSubscriptionPost
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("events")]
        public string[] Events { get; set; }

        [JsonProperty("jqlFilter")]
        public string JqlFilter { get; set; }

        [JsonProperty("exludeIssueDetails")]
        public bool ExcludeIssueDetails { get; set; }
    }
}