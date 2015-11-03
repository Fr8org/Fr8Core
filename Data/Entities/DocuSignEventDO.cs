using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class DocuSignEventDO
    {
        [Key]
        public int Id { get; set; }

        public int ExternalEventType { get; set; }
        public string RecipientId { get; set; }
        public string RecipientEmail { get; set; }
        public string EnvelopeId { get; set; }

        public string DocuSignObject { get; set; }
        public string Status { get; set; }
        public string CreateDate { get; set; }
        public string SentDate { get; set; }
        public string DeliveredDate { get; set; }
        public string CompletedDate { get; set; }
        public string Email { get; set; }
        public string EventId { get; set; }
    }
}