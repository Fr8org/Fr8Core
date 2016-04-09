using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.Manifests
{
    public class DocuSignEnvelopeCM_v2 : Manifest
    {
        [MtPrimaryKey]
        public string EnvelopeId;
        public string Status;
        public DateTime? StatusChangedDateTime;
        public string CurrentRoutingOrderId;
        public List<DocuSignRecipientStatus> Recipients = new List<DocuSignRecipientStatus>();
        public List<DocuSignTemplate> Templates = new List<DocuSignTemplate>();

        public DocuSignEnvelopeCM_v2()
              : base(Constants.MT.DocuSignEnvelope)
        {
        }
    }

    public class DocuSignRecipientStatus
    {
        public string Type;
        public string Name;
        public string Email;
        public string RecipientId;
        public string RoutingOrderId;
        public string Status;
        public List<DocuSignTab> Tabs = new List<DocuSignTab>();
    }

    public class DocuSignTab
    {
        public string TabType;
        public string Name;
        public string DocumentId;
        public string Value;
        public string TabLabel;
        public string Selected;
        public List<DocuSignTab> Items = new List<DocuSignTab>();
    }

    public class DocuSignTemplate
    {
        public string TemplateId;
        public string Name;
        public string DocumentId;
    }
}
