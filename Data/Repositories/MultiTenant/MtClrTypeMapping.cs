using System;

namespace Data.Repositories.MultiTenant
{
    class MtClrTypeMapping
    {
        public readonly Guid Id;
        public readonly string ClrType;

        public MtClrTypeMapping(Guid id, string clrType)
        {
            Id = id;
            ClrType = clrType;
        }
    }
}