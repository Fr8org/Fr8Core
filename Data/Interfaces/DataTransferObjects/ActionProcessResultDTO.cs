using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActionProcessResultDTO
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
