using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
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