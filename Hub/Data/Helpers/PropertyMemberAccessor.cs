using System;
using System.Reflection;

namespace Fr8Data.Helpers
{
    public class PropertyMemberAccessor : IMemberAccessor
    {
        private readonly PropertyInfo _propertyInfo;

        public bool CanRead => _propertyInfo.CanRead;
        public bool CanWrite => _propertyInfo.CanWrite;
        public Type MemberType => _propertyInfo.PropertyType;
        public string Name => _propertyInfo.Name;

        public PropertyMemberAccessor(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        public object GetValue(object instance)
        {
            return _propertyInfo.GetValue(instance);
        }

        public void SetValue(object instance, object value)
        {
            _propertyInfo.SetValue(instance, value);
        }

        public TAttribute GetCustomAttribute<TAttribute>()
            where TAttribute : Attribute
        {
            return _propertyInfo.GetCustomAttribute<TAttribute>();
        }
    }
}