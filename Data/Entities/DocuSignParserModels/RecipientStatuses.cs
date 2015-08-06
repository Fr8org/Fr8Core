using System.Xml.Serialization;

namespace Data.Entities.DocuSignParserModels
{
	[ XmlRoot( ElementName = "RecipientStatuses") ]
	public class RecipientStatuses
	{
		[ XmlElement( "RecipientStatus" ) ]
		public RecipientStatus[] Statuses{ get; set; }
	}
}