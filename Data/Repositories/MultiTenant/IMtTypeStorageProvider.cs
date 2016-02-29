using System;
using System.Collections.Generic;

namespace Data.Repositories.MultiTenant
{
    public interface IMtTypeStorageProvider
    {
        IEnumerable<MtTypeReference> ListTypeReferences();
        IEnumerable<MtTypePropertyReference> ListTypePropertyReferences(Guid typeId);
        MtTypeReference FindTypeReference(Type type);
        MtTypeReference FindTypeReference(Guid typeId);

        bool TryLoadType(Type clrType, out MtTypeDefinition mtType);
        void PersistType(MtTypeDefinition mtType);
        void SaveChanges();
    }
}