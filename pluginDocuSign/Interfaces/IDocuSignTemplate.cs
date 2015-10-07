using Data.Entities;
using System.Collections.Generic;
using terminal_DocuSign.DataTransferObjects;
using terminal_DocuSign.Services;

namespace terminal_DocuSign.Interfaces
{
	public interface IDocuSignTemplate
	{
		List<string> GetMappableSourceFields(DocuSignEnvelope envelope);
		IEnumerable<string> GetMappableSourceFields(string templateId);
		IEnumerable<DocuSignTemplateDTO> GetTemplates(DockyardAccountDO curDockyardAccount);
        IEnumerable<DocuSignTemplateDTO> GetTemplates(string email, string apiPassword);
		List<string> GetUserFields(DocuSignTemplateDTO curDocuSignTemplateDTO);
		DocuSignTemplateDTO GetTemplateById(string templateId);
	}
}
