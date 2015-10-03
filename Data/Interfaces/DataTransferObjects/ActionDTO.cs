using Data.Entities;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActionDTO : ActionDTOBase
    {
        public ActionDTO() : base()
        {
            CrateStorage = new CrateStorageDTO();
        }

        public int? ActionListId { get; set; }

        public CrateStorageDTO CrateStorage { get; set; }

        public FieldMappingSettingsDTO FieldMappingSettings { get; set; }

        public int? ActivityTemplateId { get; set; }

        [JsonProperty("activityTemplate")]
        public ActivityTemplateDTO ActivityTemplate { get; set; }

        [JsonProperty("isTempId")]
        public bool IsTempId { get; set; }

        [JsonProperty("action_name")]
        public string ActionName { get; set; }

        [JsonProperty("action_version")]
        public string ActionVersion { get; set; }

        public string CurrentView { get; set; }

        public int ProcessId { get; set; }
    }
}
