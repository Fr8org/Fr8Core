using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActionDesignDTO : ActionDTOBase
    {
        public int? ActionListId { get; set; }

        [JsonProperty("configurationStore")]
        public ConfigurationSettingsDTO ConfigurationStore { get; set; }

        public FieldMappingSettingsDTO FieldMappingSettings { get; set; }

        public string ParentPluginRegistration { get; set; }

        public int? ActionTemplateId { get; set; }

        [JsonProperty("actionTemplate")]
        public ActionTemplateDTO ActionTemplate { get; set; }

        [JsonProperty("isTempId")]
        public bool IsTempId { get; set; }
    }
}
