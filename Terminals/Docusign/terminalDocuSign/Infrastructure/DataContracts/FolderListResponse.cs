using System.Collections.Generic;
using Newtonsoft.Json;

namespace terminalDocuSign.Infrastructure
{
    public class FolderListResponse
    {
        [JsonProperty("folders")]
        public List<DocusignFolderInfo> Folders { get; set; }
    }
}