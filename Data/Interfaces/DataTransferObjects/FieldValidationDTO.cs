using Data.Constants;
using Data.Entities;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class FieldValidationDTO
    {
        [JsonProperty("fieldName")]
        public string FieldName { get; set; }

        [JsonProperty("crateLabel")]
        public string CrateLabel { get; set; }

        [JsonProperty("direction")]
        public ActivityDirection Direction { get; set; }

        [JsonProperty("manifestType")]
        public string ManifestType { get; set; }

        [JsonProperty("currentActionId")]
        public int CurrentActionId { get; set; }

        public FieldValidationDTO()
        {

        }

        public FieldValidationDTO(int currentActionId, string fieldName, ActivityDirection direction, string manifestType, string crateLabel)
        {
            FieldName = fieldName;
            CrateLabel = crateLabel;
            Direction = direction;
            ManifestType = manifestType;
            CurrentActionId = currentActionId;
        }

        public FieldValidationDTO(int currentActionId, string fieldName)
        {
            FieldName = fieldName;
            CurrentActionId = currentActionId;
            Direction = ActivityDirection.Up;
        }
    }
}
