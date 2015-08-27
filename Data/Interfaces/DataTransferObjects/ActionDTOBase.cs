using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
	public class ActionDTOBase
	{
        [JsonProperty("id")]
		public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
	}
}