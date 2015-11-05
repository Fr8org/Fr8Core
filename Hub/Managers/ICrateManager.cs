using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Data.Crates;
using Newtonsoft.Json.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;

namespace Hub.Managers
{
    public interface ICrateStorageUpdater : IDisposable
    {
        CrateStorage  CrateStorage { get; set; }
        void DiscardChanges();
    }

    public interface ICrateManager
    {
        //CrateDTO Create(string label, string contents, string manifestType = "", int manifestId = 0);
        //T GetContents<T>(CrateDTO crate);
        //StandardConfigurationControlsCM GetStandardConfigurationControls(CrateDTO crate);
        //StandardDesignTimeFieldsCM GetStandardDesignTimeFields(CrateDTO crate);
        //IEnumerable<CrateDTO> GetCratesByManifestType(string curManifestType, CrateStorageDTO curCrateStorageDTO);
//        IEnumerable<CrateDTO> GetCratesByLabel(string curLabel, CrateStorageDTO curCrateStorageDTO);

  //      IEnumerable<JObject> GetElementByKey<TKey>(IEnumerable<CrateDTO> searchCrates, TKey key, string keyFieldName);
        JToken SerializeToJson(Crate crate);
        JToken CrateStorageToJson(CrateStorage storage);
        string SerializeToString(Crate crate);
        
        CrateSerializationProxy SerializeToProxy(Crate crate);
        Crate Deserialize(CrateSerializationProxy proxy);

        ICrateStorageUpdater UpdateStorage(Expression<Func<JToken>> storageAccessExpression);
        ICrateStorageUpdater UpdateStorage(Expression<Func<string>> storageAccessExpression);
        CrateStorage GetStorage(string rawStorage);
        CrateStorage GetStorage(JToken rawStorage);
        bool IsEmptyStorage(string rawStorage);
        bool IsEmptyStorage(JToken rawStorage);

        //Crate CreatePayloadDataCrate(List<KeyValuePair<string, string>> curFields);
        Crate CreateAuthenticationCrate(string label, AuthenticationMode mode);
        Crate<StandardDesignTimeFieldsCM> CreateDesignTimeFieldsCrate(string label, params FieldDTO[] fields);
        Crate<StandardConfigurationControlsCM> CreateStandardConfigurationControlsCrate(string label, params ControlDefinitionDTO[] controls);
        Crate CreateStandardEventReportCrate(string label, EventReportCM eventReport);
        Crate CreateStandardEventSubscriptionsCrate(string label, params string[] subscriptions);
      //  Crate CreatePayloadDataCrate(List<KeyValuePair<string, string>> curFields);
        Crate CreateStandardTableDataCrate(string label, bool firstRowHeaders, params TableRowDTO[] table);

//        void RemoveCrateByManifestId(IList<CrateDTO> crates, int manifestId);
//        void RemoveCrateByManifestType(IList<CrateDTO> crates, string manifestType);
//        void RemoveCrateByLabel(IList<CrateDTO> crates, string label);
//
//        void ReplaceCratesByManifestType(IList<CrateDTO> sourceCrates, string manifestType,
//                                         IList<CrateDTO> newCratesContent);
//
//        void ReplaceCratesByLabel(IList<CrateDTO> sourceCrates, string label, IList<CrateDTO> newCratesContent);

        //StandardPayloadDataMS CreatePayloadDataCrate(string curObjectType);
        Crate CreatePayloadDataCrateExcel(string payloadDataObjectType, string crateLabel, StandardTableDataCM tableDataMS);

//        void AddCrate(ActionDO curActionDO, List<CrateDTO> curCrateDTOLists);
//        void AddCrate(ActionDO curActionDO, CrateDTO curCrateDTO);
//        void AddCrate(PayloadDTO payload, List<CrateDTO> curCrateDTO);
//        void AddCrate(PayloadDTO payload, CrateDTO curCrateDTO);
//        void AddOrReplaceCrate(string label, ActionDO curActionDO, CrateDTO curCrateDTO);
//        StandardConfigurationControlsCM GetConfigurationControls(ActionDO curActionDO);
//        List<CrateDTO> GetCrates(ActionDO curActionDO);
        string EmptyStorageAsStr();
        JToken EmptyStorageAsJtoken();
    }
}
