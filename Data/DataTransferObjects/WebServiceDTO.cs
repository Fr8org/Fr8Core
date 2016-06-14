using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
	public class WebServiceDTO
	{
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("iconPath")]
        public string IconPath { get; set; }
	}
}