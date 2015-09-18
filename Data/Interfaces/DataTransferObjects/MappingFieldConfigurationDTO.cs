using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class MappingFieldConfigurationDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
    }
}
