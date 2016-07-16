using Newtonsoft.Json;
using terminalStatX.Helpers;
using terminalStatX.Infrastructure;

namespace terminalStatX.DataTransferObjects.StatTypesModels
{
    public class RangeStatDTO : BaseStatDTO
    {
        public RangeStatDTO()
        {
            VisualType = StatTypes.Range;
        }

        [RenderUiProperty]
        [JsonProperty("minValue")]
        public string MinValue { get; set; }

        [RenderUiProperty]
        [JsonProperty("maxValue")]
        public string MaxValue { get; set; }

        [RenderUiProperty]
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}