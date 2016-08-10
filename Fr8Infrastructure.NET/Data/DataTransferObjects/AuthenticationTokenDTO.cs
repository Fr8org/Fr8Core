using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class AuthenticationTokenTerminalDTO
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("authenticationType")]
        public int AuthenticationType { get; set; }
        
        [JsonProperty("authTokens")]
        public List<AuthenticationTokenDTO> AuthTokens { get; set; }
    }

    public class AuthenticationTokenDTO
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("externalAccountName")]
        public string ExternalAccountName { get; set; }

        [JsonProperty("isMain")]
        public bool IsMain { get; set; }

        [JsonProperty("isSelected")]
        public bool IsSelected { get; set; }
    }

    public class AuthenticationTokenGrantDTO
    {
        [JsonProperty("actionId")]
        public Guid ActivityId { get; set; }

        [JsonProperty("authTokenId")]
        public Guid AuthTokenId { get; set; }

        [JsonProperty("isMain")]
        public bool IsMain { get; set; }
    }
}
