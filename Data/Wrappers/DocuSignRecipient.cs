using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;
using Newtonsoft.Json;
using Utilities;
using Data.Interfaces;

namespace Data.Wrappers
{
	public class DocuSignRecipient : Template, IDocuSignRecipient
	{
		private readonly DocuSignEnvelope _docusignEnvelope;

		public DocuSignRecipient()
		{
			var packager = new DocuSignPackager();
			Login = packager.Login();
		}
		public RecipientsDTO GetByTemplate(string templateId)
		{
			if (templateId == null)
				throw new ArgumentNullException("templateId");
			if (templateId == string.Empty)
				throw new ArgumentException("templateId is empty", "templateId");
			// Get template
			var jObjTemplate = GetTemplate(templateId);
			// Checking is it ok?
			DocuSignUtils.ThrowInvalidOperationExceptionIfError(jObjTemplate);

			var recipientsToken = jObjTemplate.SelectToken("recipients");
			var recipients = JsonConvert.DeserializeObject<Recipients>(recipientsToken.ToString());
			return Mapper.Map<RecipientsDTO>(recipients);
		}
		
	}
}