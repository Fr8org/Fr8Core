using System.ComponentModel.DataAnnotations;

namespace terminalDocuSign.DataTransferObjects
{
    public class DocuSignEventDTO
    {
        public int ExternalEventType { get; set; }
        public string RecipientId { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientUserName { get; set; }
        public string EnvelopeId { get; set; }

        public string DocumentName { get; set; }
        public string TemplateName { get; set; }

        public string DocuSignObject { get; set; }
        public string Status { get; set; }
        public string CreateDate { get; set; }
        public string SentDate { get; set; }
        public string DeliveredDate { get; set; }
        public string CompletedDate { get; set; }
        public string HolderEmail { get; set; }
        public string EventId { get; set; }
        public string Subject { get; set; }
    }
}