using Newtonsoft.Json;

namespace Data.Interfaces.MultiTenantObjects
{
    public class DocuSignRecipientStatusReportMTO : MultiTenantObject
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("sentDate")]
        public string SentDate { get; set; }

        [JsonProperty("deliveredDate")]
        public string DeliveredDate { get; set; }

        [JsonProperty("completedDate")]
        public string CompletedDate { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("recipientID")]
        public string RecipientID { get; set; }
    }
}
