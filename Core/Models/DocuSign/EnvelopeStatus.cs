using System.Xml.Serialization;

namespace Core.Models.DocuSign
{
	[ XmlRoot( ElementName = "EnvelopeStatus" ) ]
	public class EnvelopeStatus
	{
		[ XmlElement( "EnvelopeID" ) ]
		public string EnvelopeId{ get; set; }

		[ XmlElement( "Status" ) ]
		public string Status{ get; set; }

		[ XmlElement( "RecipientStatuses" ) ]
		public RecipientStatuses RecipientStatuses{ get; set; }
	}
}