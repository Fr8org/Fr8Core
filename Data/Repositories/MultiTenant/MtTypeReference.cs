using System;

namespace Data.Repositories.MultiTenant
{
    public class MtTypeReference
    {
        public string Alias { get; private set; }
        public Type ClrType { get; private set; }
        public Guid Id { get; private set; }

        public MtTypeReference(string @alias, Type clrType, Guid id)
        {
            Alias = alias;
            ClrType = clrType;
            Id = id;
        }
    }

    public class MtTypePropertyReference
    {
        public Guid DeclaringTypeId { get; private set; }
        public Type PropertyClrType { get; private set; }
        public Guid PropertyMtType { get; private set; }
        public string Name { get; private set; }
        public int Index { get; private set; }

        public MtTypePropertyReference(Guid declaringTypeId, Type propertyClrType, Guid propertyMtType, string name, int index)
        {
            DeclaringTypeId = declaringTypeId;
            PropertyClrType = propertyClrType;
            PropertyMtType = propertyMtType;
            Name = name;
            Index = index;
        }
    }
}
