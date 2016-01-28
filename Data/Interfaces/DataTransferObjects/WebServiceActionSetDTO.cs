using System.Collections.Generic;

namespace Data.Interfaces.DataTransferObjects
{
	public class WebServiceActionSetDTO
	{
		public string WebServiceIconPath { get; set; }
		public List<ActivityTemplateDTO> Actions { get; set; }
	}
}