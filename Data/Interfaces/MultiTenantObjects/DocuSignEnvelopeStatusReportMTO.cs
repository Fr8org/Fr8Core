namespace Data.Interfaces.MultiTenantObjects
{
    public class DocuSignEnvelopeStatusReportMTO : MultiTenantObject
    {
        public string Status { get; set; }

        public string CreateDate { get; set; }

        public string SentDate { get; set; }

        public string DeliveredDate { get; set; }

        public string CompletedDate { get; set; }
    }
}
