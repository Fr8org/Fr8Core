using Newtonsoft.Json;
using terminalStatX.Helpers;
using terminalStatX.Infrastructure;

namespace terminalStatX.DataTransferObjects.StatTypesModels
{
    public class NumberStatDTO : BaseStatDTO
    {
        public NumberStatDTO()
        {
            VisualType = StatTypes.Number;
        }

        [RenderUiProperty]
        [JsonProperty("value")]
        public string Value { get; set; }

        [RenderUiProperty]
        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [RenderUiProperty]
        [JsonProperty("postfix")]
        public string Postfix { get; set; }

        [RenderUiProperty]
        [JsonProperty("unit")]
        public string Unit { get; set; }
    }
}