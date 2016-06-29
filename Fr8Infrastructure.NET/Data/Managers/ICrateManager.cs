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
        Crate CreateAuthenticationCrate(string label, AuthenticationMode mode, bool revocation);
        Crate<KeyValueListCM> CreateDesignTimeFieldsCrate(string label, params KeyValueDTO[] fields);
        Crate<KeyValueListCM> CreateDesignTimeFieldsCrate(string label, List<KeyValueDTO> fields);
        Crate<StandardConfigurationControlsCM> CreateStandardConfigurationControlsCrate(string label, params ControlDefinitionDTO[] controls);
        Crate CreateStandardEventReportCrate(string label, EventReportCM eventReport);
        Crate CreateStandardEventSubscriptionsCrate(string label, string manufacturer, params string[] subscriptions);
        Crate CreateStandardTableDataCrate(string label, bool firstRowHeaders, params TableRowDTO[] table);
        Crate CreatePayloadDataCrate(string payloadDataObjectType, string crateLabel, StandardTableDataCM tableDataMS);
        Crate CreateOperationalStatusCrate(string label, OperationalStateCM eventReport);
        StandardPayloadDataCM TransformStandardTableDataToStandardPayloadData(string curObjectType, StandardTableDataCM tableDataMS);
        string GetFieldByKey<T>(CrateStorageDTO curCrateStorage, string findKey) where T : Manifest;
        T GetByManifest<T>(PayloadDTO payloadDTO) where T : Manifest;
        IEnumerable<KeyValueDTO> GetFields(IEnumerable<Crate> crates);
        IEnumerable<string> GetLabelsByManifestType(IEnumerable<Crate> crates, string manifestType);
        T GetContentType<T>(string crate) where T : class;
    }
}
