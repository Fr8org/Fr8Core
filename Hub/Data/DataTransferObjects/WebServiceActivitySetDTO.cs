using System.Collections.Generic;

namespace Fr8Data.DataTransferObjects
{
	public class WebServiceActivitySetDTO
	{
		public string WebServiceIconPath { get; set; }
		public List<ActivityTemplateDTO> Activities { get; set; }
        public string WebServiceName { get; set; }
	}
}