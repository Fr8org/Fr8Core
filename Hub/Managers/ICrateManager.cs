using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;

namespace Hub.Managers
{
    public interface ICrateManager
    {
        CrateDTO Create(string label, string contents, string manifestType = "", int manifestId = 0);
        T GetContents<T>(CrateDTO crate);
        StandardConfigurationControlsCM GetStandardConfigurationControls(CrateDTO crate);
        StandardDesignTimeFieldsCM GetStandardDesignTimeFields(CrateDTO crate);
        IEnumerable<CrateDTO> GetCratesByManifestType(string curManifestType, CrateStorageDTO curCrateStorageDTO);
        IEnumerable<CrateDTO> GetCratesByLabel(string curLabel, CrateStorageDTO curCrateStorageDTO);

        IEnumerable<JObject> GetElementByKey<TKey>(IEnumerable<CrateDTO> searchCrates, TKey key, string keyFieldName);
        CrateDTO CreateAuthenticationCrate(string label, AuthenticationMode mode);
        CrateDTO CreateDesignTimeFieldsCrate(string label, params FieldDTO[] fields);
        CrateDTO CreateStandardConfigurationControlsCrate(string label, params ControlDefinitionDTO[] controls);
        CrateDTO CreateStandardEventReportCrate(string label, EventReportCM eventReport);
        CrateDTO CreateStandardEventSubscriptionsCrate(string label, params string[] subscriptions);
        CrateDTO CreatePayloadDataCrate(List<KeyValuePair<string, string>> curFields);
        CrateDTO CreateStandardTableDataCrate(string label, bool firstRowHeaders, params TableRowDTO[] table);

        void RemoveCrateByManifestId(IList<CrateDTO> crates, int manifestId);
        void RemoveCrateByManifestType(IList<CrateDTO> crates, string manifestType);
        void RemoveCrateByLabel(IList<CrateDTO> crates, string label);

        void ReplaceCratesByManifestType(IList<CrateDTO> sourceCrates, string manifestType,
                                         IList<CrateDTO> newCratesContent);

        void ReplaceCratesByLabel(IList<CrateDTO> sourceCrates, string label, IList<CrateDTO> newCratesContent);

        //StandardPayloadDataMS CreatePayloadDataCrate(string curObjectType);
        CrateDTO CreatePayloadDataCrate(string payloadDataObjectType, string crateLabel, StandardTableDataCM tableDataMS);

        void AddCrate(ActionDO curActionDO, List<CrateDTO> curCrateDTOLists);
        void AddCrate(ActionDO curActionDO, CrateDTO curCrateDTO);
        void AddCrate(PayloadDTO payload, List<CrateDTO> curCrateDTO);
        void AddCrate(PayloadDTO payload, CrateDTO curCrateDTO);
        void AddOrReplaceCrate(string label, ActionDO curActionDO, CrateDTO curCrateDTO);
        StandardConfigurationControlsCM GetConfigurationControls(ActionDO curActionDO);
        List<CrateDTO> GetCrates(ActionDO curActionDO);
    }
}
