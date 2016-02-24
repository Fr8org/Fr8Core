using System.Collections.Generic;

namespace Data.Interfaces.DataTransferObjects
{
	public class WebServiceActivitySetDTO
	{
		public string WebServiceIconPath { get; set; }
		public List<ActivityTemplateDTO> Activities { get; set; }
        public string WebServiceName { get; set; }
	}
}