using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class FileDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("originalFileName")]
        public string OriginalFileName { get; set; }

        [JsonProperty("cloudStorageUrl")]
        public string CloudStorageUrl { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }
    }
}
