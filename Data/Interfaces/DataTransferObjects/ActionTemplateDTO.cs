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

        //[JsonProperty("defaultEndPoint")]
        //public string DefaultEndPoint { get; set; }

        [JsonProperty("componentActivities")]
        public string ComponentActivities { get; set; }
    }
}
