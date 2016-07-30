using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class UrlResponseDTO
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
