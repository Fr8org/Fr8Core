using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Data.Entities;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;

namespace Hub.Managers
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
        ICrateStorage FromDto(CrateStorageDTO storageDto);
        Crate FromDto(CrateDTO crateDto);
        IUpdatableCrateStorage UpdateStorage(Expression<Func<CrateStorageDTO>> storageAccessExpression);
        IUpdatableCrateStorage UpdateStorage(Expression<Func<string>> storageAccessExpression);
        string EmptyStorageAsStr();
        string CrateStorageAsStr(ICrateStorage storage);
        Crate CreateAuthenticationCrate(string label, AuthenticationMode mode, bool revocation);
        Crate<FieldDescriptionsCM> CreateDesignTimeFieldsCrate(string label, params FieldDTO[] fields);
        T GetContentType<T>(string crate) where T : class;
    }
}
