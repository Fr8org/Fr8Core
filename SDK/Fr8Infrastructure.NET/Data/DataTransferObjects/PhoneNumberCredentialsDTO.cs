using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class PhoneNumberCredentialsDTO
    {
        [JsonProperty("terminal")]
        public TerminalDTO Terminal { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("clientName")]
        public string ClientName { get; set; }

        [JsonProperty("verificationCode")]
        public string VerificationCode { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("fr8UserId")]
        public string Fr8UserId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
