using Newtonsoft.Json;

namespace fr8.Infrastructure.Data.DataTransferObjects
{
    public class ActivityNameDTO
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version  { get; set; }
    }
}
