using Newtonsoft.Json;

namespace fr8.Infrastructure.Data.DataTransferObjects
{
    public class ActivityProcessResultDTO
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
