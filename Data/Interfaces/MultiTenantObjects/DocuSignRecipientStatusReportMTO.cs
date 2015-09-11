namespace Data.Interfaces.MultiTenantObjects
{
    public class DocuSignRecipientStatusReportMTO : MultiTenantObject
    {
        public string Email { get; set; }

        public string Username { get; set; }

        public string SentDate { get; set; }

        public string DeliveredDate { get; set; }

        public string CompletedDate { get; set; }

        public string Status { get; set; }

        public string RecipientID { get; set; }
    }
}
