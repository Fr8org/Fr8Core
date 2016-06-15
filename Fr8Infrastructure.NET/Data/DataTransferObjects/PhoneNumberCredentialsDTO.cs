using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class PhoneNumberCredentialsDTO
    {
        public TerminalDTO Terminal { get; set; }
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
        [JsonProperty("clientId")]
        public string ClientId { get; set; }
        [JsonProperty("clientName")]
        public string ClientName { get; set; }
        [JsonProperty("verificationCode")]
        public string VerificationCode { get; set; }
        public string Error { get; set; }
        public string Fr8UserId { get; set; }
    }
}
