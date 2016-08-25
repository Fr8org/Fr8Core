using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class DocuSignRecipientCM : Manifest
    {
        public string RecipientStatus { get; set; }
        public string DocuSignAccountId { get; set; }
        [MtPrimaryKey]
        public string RecipientId { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientUserName { get; set; }
        public string EnvelopeId { get; set; }

        public DocuSignRecipientCM()
            : base(MT.DocuSignRecipient)
        {

        }
    }
}
