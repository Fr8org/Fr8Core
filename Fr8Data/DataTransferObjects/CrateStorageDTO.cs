using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class CrateStorageDTO
    {
        [JsonProperty("crates")]
        public CrateDTO[] Crates { get; set; }
    }
}
