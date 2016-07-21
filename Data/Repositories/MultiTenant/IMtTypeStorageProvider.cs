using System;
using System.Collections.Generic;
using Data.Repositories.SqlBased;

namespace Data.Repositories.MultiTenant
{
    public interface IMtTypeStorageProvider
    {
        IEnumerable<MtTypeReference> ListTypeReferences(ISqlConnectionProvider connectionProvider);
        IEnumerable<MtTypePropertyReference> ListTypePropertyReferences(ISqlConnectionProvider connectionProvider, Guid typeId);
        MtTypeReference FindTypeReference(ISqlConnectionProvider connectionProvider, Type type);
        MtTypeReference FindTypeReference(ISqlConnectionProvider connectionProvider, Guid typeId);
        MtTypeReference FindTypeReference(ISqlConnectionProvider connectionProvider, string alias);

        bool TryLoadType(ISqlConnectionProvider connectionProvider, Type clrType, out MtTypeDefinition mtType);
        void PersistType(ISqlConnectionProvider connectionProvider, MtTypeDefinition mtType);
        void SaveChanges(ISqlConnectionProvider connectionProvider);
    }
}