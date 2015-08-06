using System.Xml.Serialization;

namespace Data.Entities.DocuSignParserModels
{
	[ XmlRoot( ElementName = "DocuSignEnvelopeInformation", Namespace = "http://www.docusign.net/API/3.0" ) ]
	public class DocuSignEnvelopeInformation
	{
		[ XmlElement( "EnvelopeStatus" ) ]
		public EnvelopeStatus EnvelopeStatus{ get; set; }
	}
}