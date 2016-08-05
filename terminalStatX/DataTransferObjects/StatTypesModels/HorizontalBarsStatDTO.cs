using System.Collections.Generic;
using Newtonsoft.Json;
using terminalStatX.Helpers;
using terminalStatX.Infrastructure;

namespace terminalStatX.DataTransferObjects.StatTypesModels
{
    public class HorizontalBarsStatDTO : BaseStatDTO
    {
        public HorizontalBarsStatDTO()
        {
            VisualType = StatTypes.HorizontalBars;
            Items = new List<StatItemValueDTO>();
        }

        [RenderUiProperty]
        [JsonProperty("items")]
        public List<StatItemValueDTO> Items { get; set; }
    }
}