using Newtonsoft.Json;

namespace terminalFr8Core.Interfaces
{
    /// <summary>
    /// Single FilterPane condition.
    /// </summary>
    public class FilterConditionDTO
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}