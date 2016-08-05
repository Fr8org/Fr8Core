using System.Collections.Generic;
using Newtonsoft.Json;
using terminalStatX.Helpers;
using terminalStatX.Infrastructure;

namespace terminalStatX.DataTransferObjects.StatTypesModels
{
    public class PicklistStatDTO : BaseStatDTO
    {
        public PicklistStatDTO()
        {
            VisualType = StatTypes.PickList;
            Items = new List<PicklistItemDTO>();
            DynamicJsonIgnoreProperties = new[] {  "lastUpdatedDateTime",
                "notesLastUpdatedDateTime", "notes"};
        }

        [RenderUiProperty]
        [JsonProperty("label")]
        public string Label { get; set; }

        [RenderUiProperty]
        [JsonProperty("currentIndex")]
        public int CurrentIndex { get; set; }

        [RenderUiProperty]
        [JsonProperty("items")]
        public List<PicklistItemDTO> Items { get; set; }
    }

    public class PicklistItemDTO
    {
        public PicklistItemDTO()
        {
            Color = "UNKNOWN";
        }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("color")]
        public string Color { get; set; }
    }
}