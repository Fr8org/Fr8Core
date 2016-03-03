using System;
using System.Collections.Generic;

namespace Data.Repositories.MultiTenant
{
    public interface IMtTypeStorageProvider
    {
        IEnumerable<MtTypeReference> ListTypeReferences(IMtConnectionProvider connectionProvider);
        IEnumerable<MtTypePropertyReference> ListTypePropertyReferences(IMtConnectionProvider connectionProvider, Guid typeId);
        MtTypeReference FindTypeReference(IMtConnectionProvider connectionProvider, Type type);
        MtTypeReference FindTypeReference(IMtConnectionProvider connectionProvider, Guid typeId);

        bool TryLoadType(IMtConnectionProvider connectionProvider, Type clrType, out MtTypeDefinition mtType);
        void PersistType(IMtConnectionProvider connectionProvider, MtTypeDefinition mtType);
        void SaveChanges(IMtConnectionProvider connectionProvider);
    }
}