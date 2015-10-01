using DocuSign.Integrations.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pluginDocuSign.DataTransferObjects
{
    public class DocuSignTemplateDTO : TemplateInfo
    {
		 public DocuSignEnvelopeDTO EnvelopeData { get; set; }
    }
}
