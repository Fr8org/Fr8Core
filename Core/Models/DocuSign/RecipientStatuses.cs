using System.Xml.Serialization;

namespace Core.Models.DocuSign
{
	[ XmlRoot( ElementName = "RecipientStatuses") ]
	public class RecipientStatuses
	{
		[ XmlElement( "RecipientStatus" ) ]
		public RecipientStatus[] Statuses{ get; set; }
	}
}