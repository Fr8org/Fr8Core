using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;

namespace Fr8.Infrastructure.Data.Managers
{
    public interface IUpdatableCrateStorage : IDisposable, ICrateStorage
    {
        void Replace(ICrateStorage crateStorage);
        void Flush();
        void DiscardChanges();
    }

    public interface ICrateManager
    {
        CrateDTO ToDto(Crate crate);
        CrateStorageDTO ToDto(ICrateStorage storage);
        ICrateStorage FromDto(CrateStorageDTO storageDto);
        Crate FromDto(CrateDTO crateDto);
        IUpdatableCrateStorage UpdateStorage(Expression<Func<CrateStorageDTO>> storageAccessExpression);
        IUpdatableCrateStorage UpdateStorage(Expression<Func<string>> storageAccessExpression);
        bool IsEmptyStorage(CrateStorageDTO storageDto);
        string EmptyStorageAsStr();
        string CrateStorageAsStr(ICrateStorage storage);
        string CrateStorageAsStr(CrateStorageDTO storageDTO);
    }
}
