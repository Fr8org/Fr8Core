using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Fr8.Infrastructure.Data.States;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class PlanVisibilityDTO
    {
        [JsonProperty("hidden")]
        public bool Hidden { get; set; } = false;
        [JsonProperty("public")]
        public bool Public { get; set; }
    }

    public static class PlanVisibilityDTOUtils
    {
        public static PlanVisibility PlanVisibilityValue(this PlanVisibilityDTO value)
        {
            switch (value.Hidden)
            {
                case true: return PlanVisibility.Internal;
                case false: return PlanVisibility.Standard;
                default: throw new ArgumentOutOfRangeException("value");
            }
        }
    }
}
