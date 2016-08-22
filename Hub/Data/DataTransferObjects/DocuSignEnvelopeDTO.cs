using System;

namespace Fr8Data.DataTransferObjects
{
    public class DocuSignEnvelopeDTO
    {
        public string Status { get; set; }
        public string Description { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string EnvelopeId { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string OwnerName { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Shared { get; set; }

        public string TemplateId { get; set; }
    }
}
