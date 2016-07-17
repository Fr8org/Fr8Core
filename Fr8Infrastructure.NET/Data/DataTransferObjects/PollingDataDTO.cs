namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class PollingDataDTO
    {
        public bool Result { get; set; } = true;
        public bool TriggerImmediately { get; set; }
        // based on the terminal public Id and ExternalAccountId
        public string JobId { get; set; }
        public string UserId { get; set; }
        public string ExternalAccountId { get; set; }
        public string Fr8AccountId { get; set; }
        public string PollingIntervalInMinutes { get; set; }
        public string Payload { get; set; }
        public string AdditionalConfigAttributes { get; set; }
        public int RetryCounter { get; set; }
        public string AuthToken { get; set; }
        /// <summary>
        /// When this value is provided, it need to be included to the jobId when Scheduling events
        /// </summary>
        public string AdditionToJobId { get; set; }
    }
}
