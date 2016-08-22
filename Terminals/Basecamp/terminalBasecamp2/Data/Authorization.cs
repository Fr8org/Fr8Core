using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace terminalBasecamp2.Data
{
    public class Authorization
    {
        [JsonProperty("expires_at")]
        public DateTime ExpiresAt { get; set; }

        public Identity Identity { get; set; }

        public List<Account> Accounts { get; set; }
    }
}