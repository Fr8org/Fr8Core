using Newtonsoft.Json;

namespace Data.Interfaces.MultiTenantObjects
{
    public class DocuSignEnvelopeStatusReportMTO : MultiTenantObject
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("createDate")]
        public string CreateDate { get; set; }

        [JsonProperty("sentDate")]
        public string SentDate { get; set; }

        [JsonProperty("deliveredDate")]
        public string DeliveredDate { get; set; }

        [JsonProperty("completedDate")]
        public string CompletedDate { get; set; }
    }
}
