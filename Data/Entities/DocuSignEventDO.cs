using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
	public class DocuSignEventDO
	{
		[ Key ]
		public int Id{ get; set; }
		public int ExternalEventType{ get; set; }
		public string RecipientId{ get; set; }
		public string EnvelopeId{ get; set; }
	}
}