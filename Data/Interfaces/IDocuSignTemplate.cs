using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Wrappers;
using DocuSign.Integrations.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces
{
	public interface IDocuSignTemplate
	{
		List<string> GetMappableSourceFields(DocuSignEnvelope envelope);
		IEnumerable<string> GetMappableSourceFields(string templateId);
		IEnumerable<DocuSignTemplateDTO> GetTemplates(DockyardAccountDO curDockyardAccount);
		List<TextTab> GetUserFields(string templateId);
	}
}
