using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class PollingDataDTO
    {
        [JsonProperty("result")]
        public bool Result { get; set; } = true;

        [JsonProperty("triggerImmediately")]
        public bool TriggerImmediately { get; set; }
        // based on the terminal public Id and ExternalAccountId

        [JsonProperty("jobId")]
        public string JobId { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("externalAccountId")]
        public string ExternalAccountId { get; set; }

        [JsonProperty("fr8AccountId")]
        public string Fr8AccountId { get; set; }

        [JsonProperty("pollingIntervalInMinutes")]
        public string PollingIntervalInMinutes { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("additionalConfigAttributes")]
        public string AdditionalConfigAttributes { get; set; }

        [JsonProperty("retryCounter")]
        public int RetryCounter { get; set; }

        [JsonProperty("authToken")]
        public string AuthToken { get; set; }

        [JsonProperty("additionToJobId")]
        /// <summary>
        /// When this value is provided, it need to be included to the jobId when Scheduling events
        /// </summary>
        public string AdditionToJobId { get; set; }
    }
}
