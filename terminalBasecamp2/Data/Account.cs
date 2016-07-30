using Newtonsoft.Json;

namespace terminalBasecamp2.Data
{
    public class Account
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("product")]
        public string Product { get; set; }
        [JsonProperty("href")]
        public string ApiUrl { get; set; }
    }
}