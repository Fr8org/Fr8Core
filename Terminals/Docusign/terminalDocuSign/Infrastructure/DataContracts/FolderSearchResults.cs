using System.Collections.Generic;
using Newtonsoft.Json;

namespace terminalDocuSign.Infrastructure
{
    public class FolderSearchResults
    {
        [JsonProperty("resultSetSize")]
        public int ResultSetSize { get; set; }
        [JsonProperty("startPosition")]
        public int StartPosition { get; set; }
        [JsonProperty("endPosition")]
        public int EndPosition { get; set; }
        [JsonProperty("totalSetSize")]
        public int TotalSetSize { get; set; }
        [JsonProperty("previousUri")]
        public string PreviousUri { get; set; }
        [JsonProperty("nextUri")]
        public string NextUri { get; set; }
        [JsonProperty("folderItems")]
        public IList<FolderItem> FolderItems { get; set; }
    }
}