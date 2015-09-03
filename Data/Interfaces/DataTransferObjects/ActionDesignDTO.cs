using Data.Entities;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActionDesignDTO : ActionDTOBase
    {
        [JsonProperty("actionListId")]
        public int? ActionListId { get; set; }

        [JsonProperty("configurationSettings")]
        public ConfigurationSettingsDTO ConfigurationSettings { get; set; }

        [JsonProperty("fieldMappingSettings")]
        public FieldMappingSettingsDTO FieldMappingSettings { get; set; }

        [JsonProperty("parentPluginRegistration")]
        public string ParentPluginRegistration { get; set; }

        [JsonProperty("actionTemplateId")]
        public int? ActionTemplateId { get; set; }

        [JsonProperty("isTempId")]
        public bool IsTempId { get; set; }

        [JsonProperty("actionTemplate")]
        public ActionTemplateDO ActionTemplate { get; set; }
    }
}
