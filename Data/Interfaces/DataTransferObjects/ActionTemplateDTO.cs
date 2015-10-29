using Data.States;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActivityTemplateDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        public int PluginID { get; set; }

        [JsonProperty("componentActivities")]
        public string ComponentActivities { get; set; }

        public string Category { get; set; }

        [JsonProperty("minPaneWidth")]
        public int MinPaneWidth { get; set; }
    }
}
