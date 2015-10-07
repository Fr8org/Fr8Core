using System.Collections.Generic;

namespace terminal_DocuSign.DataTransferObjects
{
	public class RecipientsDTO
	{
		public List<RecipientDTO> Recipients { get; set; }
		
		public RecipientsDTO()
		{
			Recipients = new List<RecipientDTO>();
		}
	}
}
