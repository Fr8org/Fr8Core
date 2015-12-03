using System;

namespace Hub.Utilities.Binding
{
    [Flags]
    public enum BindingDirection
    {
        ToSource = 0x1,
        ToTarget = 0x2,
        TwoWay = ToSource | ToTarget,
    }
}
