using Newtonsoft.Json;
using System.Collections.Generic;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
	public class WebServiceActivitySetDTO
	{
        [JsonProperty("webServiceIconPath")]
        public string WebServiceIconPath { get; set; }

        [JsonProperty("activities")]
        public List<ActivityTemplateDTO> Activities { get; set; }

        [JsonProperty("webServiceName")]
        public string WebServiceName { get; set; }
	}
}