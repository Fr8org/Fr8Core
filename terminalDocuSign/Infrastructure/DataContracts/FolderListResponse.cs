using Newtonsoft.Json;

namespace terminalDocuSign.Infrastructure
{
    public class FolderListResponse
    {
        [JsonProperty("folders")]
        public DocusignFolderInfo[] Folders { get; set; }
    }
}