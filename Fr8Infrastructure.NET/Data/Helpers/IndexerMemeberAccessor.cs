using System;
using System.Globalization;

namespace Fr8.Infrastructure.Data.Helpers
{
    public class IndexerMemeberAccessor : IMemberAccessor
    {
        private readonly object _value;

        public bool CanRead { get; } = true;

        public bool CanWrite { get; } = false;

        public Type MemberType { get; }

        public int Id { get; }

        public string Name => string.Empty;
        
        public IndexerMemeberAccessor(Type memberType, int id, object value)
        {
            Id = id;
            _value = value;
            MemberType = memberType;
        }

        public object GetValue(object instance)
        {
            return _value;
        }

        public void SetValue(object instance, object value)
        {
            throw new InvalidOperationException("Indexer memeber accessor is read-only");
        }

        public TAttribute GetCustomAttribute<TAttribute>() 
            where TAttribute : Attribute
        {
            return null;
        }
    }
}
