using System;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class TokenResponseDTO
    {
        [JsonProperty("terminalId")]
        public Guid? TerminalId { get; set; }

        [JsonProperty("terminalName")]
        public string TerminalName { get; set; }

        [JsonProperty("authTokenId")]
        public string AuthTokenId { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
