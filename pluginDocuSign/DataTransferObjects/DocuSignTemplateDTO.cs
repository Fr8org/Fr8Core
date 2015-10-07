using DocuSign.Integrations.Client;

namespace terminal_DocuSign.DataTransferObjects
{
    public class DocuSignTemplateDTO : TemplateInfo
    {
		 public DocuSignEnvelopeDTO EnvelopeData { get; set; }
    }
}
