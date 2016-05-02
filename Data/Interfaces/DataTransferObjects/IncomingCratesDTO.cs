using System.Collections.Generic;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class IncomingCratesDTO
    {
        [JsonProperty("incomingFields")]
        public readonly List<FieldDTO> IncomingFields = new List<FieldDTO>();

        [JsonProperty("incomingCrates")]
        public readonly List<CrateDescriptionDTO> IncomingCrates = new List<CrateDescriptionDTO>();
    }
}