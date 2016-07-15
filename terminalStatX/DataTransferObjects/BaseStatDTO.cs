using System;
using Newtonsoft.Json;
using terminalStatX.Infrastructure;

namespace terminalStatX.DataTransferObjects
{
    public class BaseStatDTO
    {
        [JsonIgnore]
        public string Id { get; set; }

        [JsonIgnore]
        public string[] DynamicJsonIgnoreProperties;

        [RenderUiProperty]
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("visualType")]
        public string VisualType { get; set; }

        [JsonProperty("notes")]
        [RenderUiProperty]
        public string Notes { get; set; }

        [JsonProperty("lastUpdatedDateTime")]
        public DateTime? LastUpdatedDateTime { get; set; }

        [JsonProperty("notesLastUpdatedDateTime")]
        public DateTime? NotesLastUpdatedDateTime { get; set; }
    }
}