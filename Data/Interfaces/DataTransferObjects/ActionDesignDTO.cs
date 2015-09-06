using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActionDesignDTO : ActionDTOBase
    {
        public int? ActionListId { get; set; }

        public ConfigurationSettingsDTO ConfigurationSettings { get; set; }

        public FieldMappingSettingsDTO FieldMappingSettings { get; set; }

        public string ParentPluginRegistration { get; set; }

        public int? ActionTemplateId { get; set; }

        public bool IsTempId { get; set; }

        [JsonProperty("action_name")]
        public string ActionName { get; set; }

        [JsonProperty("action_version")]
        public string ActionVersion { get; set; }
    }
}
