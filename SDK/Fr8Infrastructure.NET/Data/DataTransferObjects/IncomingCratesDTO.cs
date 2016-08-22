using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class IncomingCratesDTO
    {
        [JsonProperty("availableCrates")]
        public readonly List<CrateDescriptionDTO> AvailableCrates = new List<CrateDescriptionDTO>();
    }
}