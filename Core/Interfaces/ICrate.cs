
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Core.Interfaces
{
    public interface ICrate 
    {
        CrateDTO Create(string label, string contents, string manifestType = "", int manifestId = 0);
        T GetContents<T>(CrateDTO crate);
        IEnumerable<JObject> GetElementByKey<TKey>(IEnumerable<CrateDTO> searchCrates, TKey key, string keyFieldName);
        CrateDTO CreateDesignTimeFieldsCrate(string label, params FieldDTO[] fields);
        CrateDTO CreateStandardConfigurationControlsCrate(string label, params FieldDefinitionDTO[] controls);

        void RemoveCrateByManifestId(IList<CrateDTO> crates, int manifestId);
        void RemoveCrateByManifestType(IList<CrateDTO> crates, string manifestType);
        void RemoveCrateByLabel(IList<CrateDTO> crates, string label);
    }
}
