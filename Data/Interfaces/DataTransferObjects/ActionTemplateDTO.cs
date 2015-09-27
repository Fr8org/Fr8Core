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

        [JsonProperty("version")]
        public string Version { get; set; }

        public int PluginID { get; set; }

        [JsonProperty("componentActivities")]
        public string ComponentActivities { get; set; }

        [JsonProperty("category")]
        public ActivityCategory Category { get; set; }
    }
}
