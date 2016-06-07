using Newtonsoft.Json;

namespace fr8.Infrastructure.Data.DataTransferObjects
{
    public class ProfileDTO
    { 
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
