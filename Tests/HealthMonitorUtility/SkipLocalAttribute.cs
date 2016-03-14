using System;

namespace HealthMonitorUtility
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SkipLocalAttribute : Attribute
    {
    }
}
