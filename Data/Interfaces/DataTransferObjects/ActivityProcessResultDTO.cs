using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActivityProcessResultDTO
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
