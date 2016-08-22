
using System;

namespace Fr8.Infrastructure.Data.Manifests
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MtPrimaryKeyAttribute : Attribute
    {
    }
}