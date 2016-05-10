using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class ActivityProcessResultDTO
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
