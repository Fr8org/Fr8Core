using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace terminalDocuSign.DataTransferObjects
{
	public class RecipientDTO
	{
		public string RecipientId { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }
		public string Role { get; set; }
	}
}
