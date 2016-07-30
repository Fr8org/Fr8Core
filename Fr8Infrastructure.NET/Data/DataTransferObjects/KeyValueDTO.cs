using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class KeyValueDTO
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        public KeyValueDTO(string key, string value)
        {
            Key = key;
            Value = value;
        }
        
        public KeyValueDTO()
        {
        }

        public KeyValueDTO Clone()
        {
            return new KeyValueDTO(Key, Value);
        }
    }
}
