using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActionRegistrationDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("actionType")]
        public string ActionType { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("parentPluginRegistration")]
        public string ParentPluginRegistration { get; set; }
    }
}
