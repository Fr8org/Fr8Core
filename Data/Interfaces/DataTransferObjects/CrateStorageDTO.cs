using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class CrateStorageDTO
    {
        [JsonProperty("crates")]
        public CrateDTO[] Crates { get; set; }
    }
}
