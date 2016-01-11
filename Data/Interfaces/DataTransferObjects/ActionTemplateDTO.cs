using Data.Control;
using Data.States;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

        [JsonProperty("webService")]
        public WebServiceDTO WebService { get; set; }

        [JsonProperty("terminal")]
        public TerminalDTO Terminal { get; set; }
        public int TerminalId { get; set; }

        [JsonProperty("componentActivities")]
        public string ComponentActivities { get; set; }
        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ActivityCategory Category { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ActivityType Type { get; set; }

        [JsonProperty("minPaneWidth")]
        public int MinPaneWidth { get; set; }

        public ActivityTemplateDTO()
        {
            Type = ActivityType.Standard;
        }

        public string Description { get; set; }

        public bool NeedsAuthentication { get; set; }

        [JsonProperty("help")]
        public HelpControlDTO Help { get; set; }
    }
}
