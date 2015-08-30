using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActionDesignDTO : ActionDTOBase
    {
        [JsonProperty("userLabel")]
        public string UserLabel { get; set; }

        [JsonProperty("actionListId")]
        public int? ActionListId { get; set; }

        [JsonProperty("configurationSettings")]
        public string ConfigurationSettings { get; set; }

        [JsonProperty("fieldMappingSettings")]
        public FieldMappingSettingsDTO FieldMappingSettings { get; set; }

        [JsonProperty("parentPluginRegistration")]
        public string ParentPluginRegistration { get; set; }

        [JsonProperty("docuSignTemplateId")]
	    public string DocuSignTemplateId { get; set; }

        [JsonProperty("actionTemplateId")]
        public int ActionTemplateId { get; set; }
    }
}
