using Newtonsoft.Json;

namespace terminalStatX.DataTransferObjects
{
    public class StatXAuthResponseDTO
    {
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
        [JsonProperty("clientId")]
        public string ClientId { get; set; }
        [JsonProperty("clientName")]
        public string ClientName { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}