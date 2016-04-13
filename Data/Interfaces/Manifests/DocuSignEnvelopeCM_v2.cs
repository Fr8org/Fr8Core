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
              : base(Constants.MT.DocuSignEnvelope_v2)
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
        public List<DocuSignTabStatus> Tabs = new List<DocuSignTabStatus>();
    }

    public class DocuSignTabStatus
    {
        public string TabType;
        public string Name;
        public string DocumentId;
        public string Value;
        public string TabLabel;
        public string Selected;
        public List<DocuSignTabStatus> Items = new List<DocuSignTabStatus>();
    }

    public class DocuSignTemplate
    {
        public string TemplateId;
        public string Name;
        public string DocumentId;
    }
}
