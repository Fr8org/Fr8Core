using Newtonsoft.Json;

namespace terminalGoogle.DataTransferObjects
{
    public class GoogleFormField
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("index")]
        public int Index { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}