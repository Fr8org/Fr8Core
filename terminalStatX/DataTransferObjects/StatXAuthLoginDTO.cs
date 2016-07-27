using Newtonsoft.Json;

namespace terminalStatX.DataTransferObjects
{
    public class StatXAuthLoginDTO
    {
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
        [JsonProperty("clientName")]
        public string ClientName { get; set; }
    }
}