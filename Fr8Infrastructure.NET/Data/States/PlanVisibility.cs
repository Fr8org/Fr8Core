using Newtonsoft.Json;
using System;

namespace Fr8.Infrastructure.Data.States
{
    public enum PlanVisibility
    {
        Standard = 1,
        Internal = 2
    }

    public static class PlanVisibilityUtils
    {
        public static bool BooleanValue(this PlanVisibility value)
        {
            switch (value)
            {
                case PlanVisibility.Internal: return true;
                case PlanVisibility.Standard: return false;
                default: throw new ArgumentOutOfRangeException("value");
            }
        }
    }
}
