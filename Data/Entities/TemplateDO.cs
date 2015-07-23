using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DocuSign.Integrations.Client;
using Newtonsoft.Json.Linq;

namespace Data.Entities
{
	public class TemplateDO: BaseDO
	{
		[ Key ]
		public int Id{ get; set; }

		private readonly Template _docusignTemplate;

		public TemplateDO( Template docusignTemplate )
		{
			this._docusignTemplate = docusignTemplate;
		}

		public bool CreateTemplate( List< byte[] > fileBytesList, List< string > fileNames, string templateName )
		{
			return this._docusignTemplate.CreateTemplate( fileBytesList, fileNames, templateName );
		}

		public JObject GetTemplate( string templateId )
		{
			return this._docusignTemplate.GetTemplate( templateId );
		}

		public byte[] GetTemplatePreview( string templateId )
		{
			return this._docusignTemplate.GetTemplatePreview( templateId );
		}

		public List< TemplateInfo > GetTemplates()
		{
			return this._docusignTemplate.GetTemplates();
		}
	}
}