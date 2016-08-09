using Newtonsoft.Json;
using System;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class AuthorizationTokenDTO
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("externalAccountId")]
        public string ExternalAccountId { get; set; }

        [JsonProperty("externalAccountName")]
        public string ExternalAccountName { get; set; }

        [JsonProperty("externalDomainId")]
        public string ExternalDomainId { get; set; }

        [JsonProperty("externalDomainName")]
        public string ExternalDomainName { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("externalStateToken")]
        public string ExternalStateToken { get; set; }

        [JsonProperty("additionalAttributes")]
        public string AdditionalAttributes { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("expiresAt")]
        public DateTime? ExpiresAt { get; set; }

        [JsonProperty("authCompletedNotificationRequired")]
        public bool AuthCompletedNotificationRequired { get; set; }

        //TODO remove this
        [JsonProperty("terminalId")]
        public Guid TerminalID { get; set; }
    }
}
