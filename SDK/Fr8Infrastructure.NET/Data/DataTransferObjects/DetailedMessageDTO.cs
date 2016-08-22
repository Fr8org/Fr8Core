using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class DetailedMessageDTO
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("messageDetails")]
        public string MessageDetails { get; set; }
    }
}
