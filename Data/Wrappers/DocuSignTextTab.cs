using DocuSign.Integrations.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Wrappers
{
	public class DocuSignTextTab : Template
	{
		public DocuSignTextTab()
		{
			var packager = new DocuSignPackager();

			Login = packager.Login();
		}
		public List<TextTab> GetUserFields(string templateId)
		{
			if (templateId == null)
				throw new ArgumentNullException("templateId");
			if (templateId == string.Empty)
				throw new ArgumentException("templateId is empty", "templateId");
			// Get template
			var jObjTemplate = GetTemplate(templateId);
			// Checking is it ok?
			DocuSignUtils.ThrowInvalidOperationExceptionIfError(jObjTemplate);

			Recipients recipient = DocuSignUtils.GetRecipientsFromTemplate(jObjTemplate);
			// TODO Do we need to get textTabs for other types of recipients?
			var allSigners = recipient.signers.Concat(recipient.agents)
				.Concat(recipient.carbonCopies)
				.Concat(recipient.certifiedDeliveries)
				.Concat(recipient.editors)
				.Concat(recipient.inPersonSigners)
				.Concat(recipient.intermediaries).ToArray();

			List<TextTab> textTabs = new List<TextTab>();
			Array.ForEach(allSigners, x =>
			{
				if (x.tabs != null && x.tabs.textTabs != null)
					textTabs.AddRange(x.tabs.textTabs);
			});
			
			return textTabs;
		}
		
	}
}
