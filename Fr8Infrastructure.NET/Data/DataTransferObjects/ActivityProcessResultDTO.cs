using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class ActivityProcessResultDTO
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
