using Newtonsoft.Json;

namespace terminalDocuSign.Infrastructure
{
    public class DocusignFolderInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("folderId")]
        public string FolderId { get; set; }
    }
}