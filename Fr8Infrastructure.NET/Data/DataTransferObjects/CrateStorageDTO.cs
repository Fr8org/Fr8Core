using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class CrateStorageDTO
    {
        [JsonProperty("crates")]
        public CrateDTO[] Crates { get; set; }
    }
}
