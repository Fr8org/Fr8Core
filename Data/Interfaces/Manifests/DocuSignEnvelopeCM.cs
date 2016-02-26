using System;

namespace Data.Interfaces.Manifests
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

        public DateTime? StatusChangedDateTime { get; set; }

        public DocuSignEnvelopeCM()
              : base(Constants.MT.DocuSignEnvelope)
        {

        }
    }
}
