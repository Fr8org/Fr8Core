using System.Collections.Generic;
using Newtonsoft.Json;
using terminalStatX.Helpers;
using terminalStatX.Infrastructure;

namespace terminalStatX.DataTransferObjects.StatTypesModels
{
    public class CheckListStatDTO : BaseStatDTO
    {
        public CheckListStatDTO()
        {
            VisualType = StatTypes.CheckList;
            Items = new List<CheckListItemDTO>();
        }

        [RenderUiProperty]
        [JsonProperty("items")]
        public List<CheckListItemDTO> Items { get; set; }
    }

    public class CheckListItemDTO
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("checked")]
        public bool Checked { get; set; }
    }
}