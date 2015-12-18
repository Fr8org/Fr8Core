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

        [JsonProperty("authenticationType")]
        public int AuthenticationType { get; set; }

        [JsonProperty("authTokens")]
        public List<ManageAuthToken_AuthToken> AuthTokens { get; set; }
    }

    public class ManageAuthToken_AuthToken
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("externalAccountName")]
        public string ExternalAccountName { get; set; }

        [JsonProperty("isMain")]
        public bool IsMain { get; set; }
    }

    public class ManageAuthToken_Terminal_Action
    {
        [JsonProperty("actionId")]
        public Guid ActionId { get; set; }

        [JsonProperty("terminal")]
        public ManageAuthToken_Terminal Terminal { get; set; }
    }

    public class ManageAuthToken_Apply
    {
        [JsonProperty("actionId")]
        public Guid ActionId { get; set; }

        [JsonProperty("authTokenId")]
        public Guid AuthTokenId { get; set; }
    }
}
