using Newtonsoft.Json;
using System;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class CredentialsDTO
    {
        [JsonProperty("terminal")]
        public TerminalSummaryDTO Terminal { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("isDemoAccount")]
        public bool IsDemoAccount { get; set;  }

        [JsonProperty("fr8UserId")]
        public string Fr8UserId { get; set; }
    }
}
