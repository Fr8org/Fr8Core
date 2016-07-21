using System;

namespace Fr8Data.States
{
    [Flags]
    public enum AvailabilityType
    {
        NotSet = 0,
        Configuration = 0x1,
        RunTime = 0x2,
        Always = Configuration | RunTime
    }

}
