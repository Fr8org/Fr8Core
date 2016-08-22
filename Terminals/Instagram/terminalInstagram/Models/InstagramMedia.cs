using Newtonsoft.Json;

namespace terminalInstagram.Models
{
    public class InstagramMedia
    {
        [JsonProperty("changed_aspect")]
        public string ChangedAspect { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("object_id")]
        public string ObjectId { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("subscription_id")]
        public string SubscriptionId { get; set; }

        [JsonProperty("data")]
        public Data MediaData { get; set; }
    }

    public class Data
    {
        [JsonProperty("media_id")]
        public string MediaId { get; set; }
    }
}