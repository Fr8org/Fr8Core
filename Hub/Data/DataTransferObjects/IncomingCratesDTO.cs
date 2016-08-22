using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class IncomingCratesDTO
    {
        [JsonProperty("availableFields")]
        public readonly List<FieldDTO> AvailableFields = new List<FieldDTO>();

        [JsonProperty("availableCrates")]
        public readonly List<CrateDescriptionDTO> AvailableCrates = new List<CrateDescriptionDTO>();
    }
}