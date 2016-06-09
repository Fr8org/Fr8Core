using System;
using Fr8Data.Constants;
using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
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
        public Guid CurrentActivityId { get; set; }

        public FieldValidationDTO()
        {

        }

        public FieldValidationDTO(Guid currentActionId, string fieldName, ActivityDirection direction, string manifestType, string crateLabel)
        {
            FieldName = fieldName;
            CrateLabel = crateLabel;
            Direction = direction;
            ManifestType = manifestType;
            CurrentActivityId = currentActionId;
        }

        public FieldValidationDTO(Guid currentActionId, string fieldName)
        {
            FieldName = fieldName;
            CurrentActivityId = currentActionId;
            Direction = ActivityDirection.Up;
        }
    }
}
