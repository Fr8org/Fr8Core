using System;
using System.Reflection;

namespace Fr8.Infrastructure.Data.Helpers
{
    public class FieldMemberAccessor : IMemberAccessor
    {
        private readonly FieldInfo _fieldInfo;

        public bool CanRead => true;
        public bool CanWrite => true;
        public Type MemberType => _fieldInfo.FieldType;
        public string Name => _fieldInfo.Name;

        public FieldMemberAccessor(FieldInfo fieldInfo)
        {
            _fieldInfo = fieldInfo;
        }

        public object GetValue(object instance)
        {
            return _fieldInfo.GetValue(instance);
        }

        public void SetValue(object instance, object value)
        {
            _fieldInfo.SetValue(instance, value);
        }

        public TAttribute GetCustomAttribute<TAttribute>() 
            where TAttribute : Attribute
        {
            return _fieldInfo.GetCustomAttribute<TAttribute>();
        }
    }
}