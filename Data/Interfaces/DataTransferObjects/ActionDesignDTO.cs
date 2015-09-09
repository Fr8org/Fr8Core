using Data.Entities;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActionDesignDTO : ActionDTOBase
    {
        public int? ActionListId { get; set; }

        [JsonProperty("configurationStore")]
        public CrateStorageDTO CrateStorage { get; set; }

        public FieldMappingSettingsDTO FieldMappingSettings { get; set; }

        public int? ActionTemplateId { get; set; }

        [JsonProperty("actionTemplate")]
        public ActionTemplateDTO ActionTemplate { get; set; }

        [JsonProperty("isTempId")]
        public bool IsTempId { get; set; }

        [JsonProperty("action_name")]
        public string ActionName { get; set; }

        [JsonProperty("action_version")]
        public string ActionVersion { get; set; }
    }
}
