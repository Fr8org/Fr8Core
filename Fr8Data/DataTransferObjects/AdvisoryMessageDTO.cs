using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class AdvisoryMessageDTO
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
