using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ManageAuthToken_Terminal
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("authTokens")]
        public List<ManageAuthToken_AuthToken> AuthTokens { get; set; }
    }

    public class ManageAuthToken_AuthToken
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("externalAccountName")]
        public string ExternalAccountName { get; set; }
    }
}
