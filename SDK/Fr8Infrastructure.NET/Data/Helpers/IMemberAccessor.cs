using System;

namespace Fr8.Infrastructure.Data.Helpers
{
    public interface IMemberAccessor
    {
        bool CanRead { get; }
        bool CanWrite { get; }
        
        Type MemberType { get; }
        string Name { get; }
        object GetValue(object instance);
        void SetValue(object instance, object value);
        TAttribute GetCustomAttribute<TAttribute>() where TAttribute : Attribute;
    }
}