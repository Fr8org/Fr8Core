using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class KeyValueDTO
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
