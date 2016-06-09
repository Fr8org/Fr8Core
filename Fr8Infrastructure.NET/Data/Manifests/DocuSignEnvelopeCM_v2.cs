using System;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class DocuSignEnvelopeCM_v2 : Manifest
    {
        [MtPrimaryKey]
        public string EnvelopeId { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public DateTime? StatusChangedDateTime { get; set; }
        public string CurrentRoutingOrderId { get; set; }
        public List<DocuSignRecipientStatus> Recipients { get; set; } = new List<DocuSignRecipientStatus>();
        public List<DocuSignTemplate> Templates { get; set; } = new List<DocuSignTemplate>();
        public string ExternalAccountId { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? SentDate { get; set; }

        public DocuSignEnvelopeCM_v2()
              : base(MT.DocuSignEnvelope_v2)
        {
        }

        public DocuSignRecipientStatus GetCurrentRecipient()
        {
            return this.Recipients?.OrderByDescending(a => a.RoutingOrderId).FirstOrDefault(a => a.RoutingOrderId == this.CurrentRoutingOrderId);
        }
    }

    public class DocuSignRecipientStatus
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string RecipientId { get; set; }
        public string RoutingOrderId { get; set; }
        public string Status { get; set; }
        public List<DocuSignTabStatus> Tabs { get; set; } = new List<DocuSignTabStatus>();
    }

    public class DocuSignTabStatus
    {
        public string TabType { get; set; }
        public string Name { get; set; }
        public string DocumentId { get; set; }
        public string Value { get; set; }
        public string TabLabel { get; set; }
        public string Selected { get; set; }
        public List<DocuSignTabStatus> Items { get; set; } = new List<DocuSignTabStatus>();
    }

    public class DocuSignTemplate
    {
        /// <summary>If event comes from connect - templateId will be empty </summary>
        public string TemplateId { get; set; }
        public string Name { get; set; }
        public string DocumentId { get; set; }
    }
}
