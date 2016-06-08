using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.Crates;

namespace Data.Repositories.MultiTenant
{
    public class MtTypeDefinition
    {
        public readonly List<MtPropertyInfo> Properties;
        public readonly Type ClrType;
        public readonly string Alias;
        public readonly Guid Id;
        public readonly bool IsComplexType;
        public readonly bool IsPrimitive;

        public MtTypeDefinition(List<MtPropertyInfo> properties, Guid id, bool isComplexType, bool isPrimitive, Type clrType)
        {
            Properties = properties;
            Id = id;
            IsComplexType = isComplexType;
            IsPrimitive = isPrimitive;
            ClrType = clrType;

            CrateManifestType manifestType;

            if (ManifestDiscovery.Default.TryGetManifestType(clrType, out manifestType))
            {
                Alias = manifestType.Type;
            }
        }

        public static MtTypeDefinition MakeComplexType(Guid id, Type clrType)
        {
            return new MtTypeDefinition(null, id, true, false, clrType);
        }

        public static MtTypeDefinition MakePrimitive(Guid id, Type clrType)
        {
            return new MtTypeDefinition(null, id, false, true, clrType);
        }

        public static MtTypeDefinition MakeType(Guid id, Type clrType)
        {
            return new MtTypeDefinition(new List<MtPropertyInfo>(), id, false, false, clrType);
        }

    }
}