using Newtonsoft.Json;

namespace terminalDocuSign.Infrastructure
{
    public class CustomField
    {
        [JsonProperty("fieldId")]
        public string FieldId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("show")]
        public string Show { get; set; }
        [JsonProperty("required")]
        public string Required { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}