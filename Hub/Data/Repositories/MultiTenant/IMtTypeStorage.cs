using System;
using Data.Repositories.SqlBased;

namespace Data.Repositories.MultiTenant
{
    internal interface IMtTypeStorage
    {
        MtTypeDefinition ResolveType(ISqlConnectionProvider connectionProvider, Type clrType, IMtTypeStorageProvider typeStorageProvider, bool storeIfNew);
        ITypeTransactionLock AccureTypeTransactionLock();
    }
}