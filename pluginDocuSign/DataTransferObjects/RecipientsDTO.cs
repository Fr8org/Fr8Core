using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pluginDocuSign.DataTransferObjects
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
