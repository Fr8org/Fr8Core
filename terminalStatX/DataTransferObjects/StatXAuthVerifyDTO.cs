using Newtonsoft.Json;

namespace terminalStatX.DataTransferObjects
{
    public class StatXAuthVerifyDTO
    {
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
        [JsonProperty("clientId")]
        public string ClientId { get; set; }
        [JsonProperty("verificationCode")]
        public string VerificationCode { get; set; }
    }
}