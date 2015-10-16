using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;

namespace Core.Managers
{
    public interface ICrateManager
    {
        CrateDTO Create(string label, string contents, string manifestType = "", int manifestId = 0);
        T GetContents<T>(CrateDTO crate);
        StandardConfigurationControlsCM GetStandardConfigurationControls(CrateDTO crate);
        StandardDesignTimeFieldsCM GetStandardDesignTimeFields(CrateDTO crate);

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
    }
}
