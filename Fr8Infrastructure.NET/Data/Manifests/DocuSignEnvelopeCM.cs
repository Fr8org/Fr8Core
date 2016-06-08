using System;
using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class DocuSignEnvelopeCM : Manifest
    {
        public string Status { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        [MtPrimaryKey]
        public string EnvelopeId { get; set; }
        public string ExternalAccountId { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string OwnerName { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Shared { get; set; }

        public DateTime? StatusChangedDateTime { get; set; }

        public DocuSignEnvelopeCM()
              : base(MT.DocuSignEnvelope)
        {

        }
    }
}
