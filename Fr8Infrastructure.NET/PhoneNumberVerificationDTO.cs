using System;
using Newtonsoft.Json;

namespace Fr8.Infrastructure
{
    public class PhoneNumberVerificationDTO
    {

        [JsonProperty("terminalId")]
        public Guid TerminalId { get; set; }

        [JsonProperty("terminalName")]
        public string TerminalName { get; set; }

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("clientName")]
        public string ClientName { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
