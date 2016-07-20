using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;

namespace Fr8Data.Managers
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
        Crate<ManifestDescriptionCM> CreateManifestDescriptionCrate(string label, string name, string id, AvailabilityType availability);
        Crate<FieldDescriptionsCM> CreateDesignTimeFieldsCrate(string label, params FieldDTO[] fields);
        Crate<FieldDescriptionsCM> CreateDesignTimeFieldsCrate(string label, List<FieldDTO> fields);
        Crate<FieldDescriptionsCM> CreateDesignTimeFieldsCrate(string label, List<FieldDTO> fields, AvailabilityType availability);
        Crate<FieldDescriptionsCM> CreateDesignTimeFieldsCrate(string label, AvailabilityType availability, params FieldDTO[] fields);
        Crate<StandardConfigurationControlsCM> CreateStandardConfigurationControlsCrate(string label, params ControlDefinitionDTO[] controls);
        Crate CreateStandardEventReportCrate(string label, EventReportCM eventReport);
        Crate CreateStandardEventSubscriptionsCrate(string label, string manufacturer, params string[] subscriptions);
        Crate CreateStandardTableDataCrate(string label, bool firstRowHeaders, params TableRowDTO[] table);
        Crate CreatePayloadDataCrate(string payloadDataObjectType, string crateLabel, StandardTableDataCM tableDataMS);
        Crate CreateOperationalStatusCrate(string label, OperationalStateCM eventReport);
        StandardPayloadDataCM TransformStandardTableDataToStandardPayloadData(string curObjectType, StandardTableDataCM tableDataMS);
        string GetFieldByKey<T>(CrateStorageDTO curCrateStorage, string findKey) where T : Manifest;
        //void AddLogMessage(string label, List<LogItemDTO> logItemList, ICrateStorage payload);
        T GetByManifest<T>(PayloadDTO payloadDTO) where T : Manifest;
        IEnumerable<FieldDTO> GetFields(IEnumerable<Crate> crates);
        IEnumerable<string> GetLabelsByManifestType(IEnumerable<Crate> crates, string manifestType);
        FieldDescriptionsCM MergeContentFields(List<Crate<FieldDescriptionsCM>> curCrates);
        T GetContentType<T>(string crate) where T : class;
    }
}
