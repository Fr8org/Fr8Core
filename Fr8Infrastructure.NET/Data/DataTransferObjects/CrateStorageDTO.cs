using Newtonsoft.Json;

namespace fr8.Infrastructure.Data.DataTransferObjects
{
    public class CrateStorageDTO
    {
        [JsonProperty("crates")]
        public CrateDTO[] Crates { get; set; }
    }
}
