using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Wrappers;
using DocuSign.Integrations.Client;
using Newtonsoft.Json.Linq;

namespace Data.Entities
{
	public class TemplateDO: BaseDO
	{
		[ Key ]
		public int Id{ get; set; }

		private readonly DocuSignTemplate _docusignTemplate;

        [NotMapped]
        public DocuSignTemplate DocuSignTemplate { get; set; }

        public TemplateDO( DocuSignTemplate docusignTemplate )
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