using AutoMapper;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;
using Newtonsoft.Json;
using pluginDocuSign.DataTransferObjects;
using pluginDocuSign.Infrastructure;
using pluginDocuSign.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace pluginDocuSign.Services
{
	public class DocuSignRecipient : Template, IDocuSignRecipient
	{
		private readonly DocuSignEnvelope _docusignEnvelope;

		public DocuSignRecipient()
		{
			var docuSignPackager = new DocuSignPackager();
			Login = docuSignPackager.Login();
		}
		public Recipients GetByTemplate(string templateId)
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
			return recipients;
		}

	}
}