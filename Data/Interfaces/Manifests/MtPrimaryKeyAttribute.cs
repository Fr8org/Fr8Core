
using System;

namespace Data.Interfaces.Manifests
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MtPrimaryKeyAttribute : Attribute
    {
    }
}